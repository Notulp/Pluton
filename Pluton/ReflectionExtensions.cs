using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Pluton
{
    public static class ReflectionExtensions
    {
        //Instanced
        public static object CallMethod(this object obj, string methodName, params object[] args)
        {
            var metInf = GetMethodInfo(obj, methodName);

            if (metInf == null)
                throw new Exception(String.Format("Couldn't find method '{0}' using reflection.", methodName));

            if (metInf is MethodInfo)
                return metInf.As<MethodInfo>().Invoke(obj, args);
            
            return (object)null;
        }

        public static object CallMethodOnBase(this object obj, string methodName, params object[] args)
        {
            Type Base = obj.GetType().BaseType;
            if (Base != null)
            {
                return CallMethodOnBase(obj, GetMethodInfo(Base, methodName), args);
            }
            return null;
        }

        public static object CallMethodOnBase(this object obj, Type Base, string methodname, params object[] args)
        {
            return CallMethodOnBase(obj, GetMethodInfo(Base, methodname), args);
        }

        public static object CallMethodOnBase(this object obj, MethodInfo method, params object[] args)
        {
            var parameters = method.GetParameters();

            if (parameters.Length == 0) {
                if (args != null && args.Length != 0) 
                    throw new Exception("Arguments count doesn't match");
            } else {
                if (parameters.Length != args.Length)
                    throw new Exception("Arguments count doesn't match");
            }

            Type returnType = null;
            if (method.ReturnType != typeof(void)) {
                returnType = method.ReturnType;
            }

            var type = obj.GetType();
            var dynamicMethod = new DynamicMethod("", returnType, 
                                                  new Type[] { type, typeof(Object) }, type);

            var iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];

                iLGenerator.Emit(OpCodes.Ldarg_1);

                iLGenerator.Emit(OpCodes.Ldc_I4_S, i);
                iLGenerator.Emit(OpCodes.Ldelem_Ref);

                var parameterType = parameter.ParameterType;
                if (parameterType.IsPrimitive) {
                    iLGenerator.Emit(OpCodes.Unbox_Any, parameterType);
                } else if (parameterType == typeof(Object)) {
                } else {
                    iLGenerator.Emit(OpCodes.Castclass, parameterType);
                }
            }

            iLGenerator.Emit(OpCodes.Call, method);
            iLGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.Invoke(null, new [] { obj, args });
        }

        public static object GetFieldValue(this object obj, string fieldName)
        {
            var memInf = GetFieldInfo(obj, fieldName);

            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));

            if (memInf is System.Reflection.PropertyInfo)
                return memInf.As<System.Reflection.PropertyInfo>().GetValue(obj, null);

            if (memInf is System.Reflection.FieldInfo)
                return memInf.As<System.Reflection.FieldInfo>().GetValue(obj);

            throw new Exception();
        }

        public static object GetFieldValueChain(this object obj, params string[] args)
        {
            foreach (string arg in args) {
                obj = obj.GetFieldValue(arg);
            }
            return obj;
        }

        public static void SetFieldValue(this object obj, string fieldName, object newValue)
        {
            var memInf = GetFieldInfo(obj, fieldName);
            
            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));

            if (memInf is System.Reflection.PropertyInfo)
                memInf.As<System.Reflection.PropertyInfo>().SetValue(obj, newValue, null);
            else if (memInf is System.Reflection.FieldInfo)
                memInf.As<System.Reflection.FieldInfo>().SetValue(obj, newValue);
            else
                throw new System.Exception();
        }

        private static MethodInfo GetMethodInfo(Type classType, string methodName)
        {
            return classType.GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static);
        }

        private static MethodInfo GetMethodInfo(object obj, string methodName)
        {
            return GetMethodInfo(obj.GetType(), methodName);
        }

        private static MemberInfo GetFieldInfo(Type objType, string fieldName)
        {
            var prps = new List<System.Reflection.PropertyInfo>();

            prps.Add(objType.GetProperty(fieldName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static));

            prps = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(prps, i => !ReferenceEquals(i, null)));

            if (prps.Count != 0)
                return prps[0];

            var flds = new System.Collections.Generic.List<System.Reflection.FieldInfo>();

            flds.Add(objType.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Public  | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static));          

            flds = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(flds, i => !ReferenceEquals(i, null)));

            if (flds.Count != 0)
                return flds[0];

            // if not found on the current type, check the base
            if (objType.BaseType != null) {
                return GetFieldInfo (objType.BaseType, fieldName);
            }

            return null;
        }

        private static MemberInfo GetFieldInfo(object obj, string fieldName)
        {
            return GetFieldInfo(obj.GetType(), fieldName);
        }

        //Static
        public static void CallStaticMethod(this Type classType, string methodName, params object[] args)
        {
            var metInf = GetMethodInfo(classType, methodName);

            if (metInf == null)
                throw new Exception(String.Format("Couldn't find method '{0}' using reflection.", methodName));

            if (metInf is MethodInfo) {
                MethodInfo meta = metInf.As<MethodInfo>();
                meta.Invoke(null, args);
            }
        }

        public static object GetStaticFieldValue(this Type classType, string fieldName)
        {
            var memInf = GetFieldInfo(classType, fieldName);

            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));

            if (memInf is System.Reflection.PropertyInfo)
                return memInf.As<System.Reflection.PropertyInfo>().GetValue(null, null);

            if (memInf is System.Reflection.FieldInfo)
                return memInf.As<System.Reflection.FieldInfo>().GetValue(null);

            throw new Exception();
        }

        public static void SetFieldValueValue(this Type classType, string fieldName, object newValue)
        {
            var memInf = GetFieldInfo(classType, fieldName);

            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));           

            if (memInf is System.Reflection.PropertyInfo)
                memInf.As<System.Reflection.PropertyInfo>().SetValue(null, newValue, null);
            else if (memInf is System.Reflection.FieldInfo)
                memInf.As<System.Reflection.FieldInfo>().SetValue(null, newValue);
            else
                throw new System.Exception();
        }

        [System.Diagnostics.DebuggerHidden]
        private static T As<T>(this object obj)
        {
            return (T)obj;
        }
    }
}
