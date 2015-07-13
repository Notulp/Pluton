using System;
using System.Collections.Generic;
using System.Reflection;

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

            if (metInf != null) {
                MethodInfo meta = metInf.As<MethodInfo>();
                return meta.Invoke(obj, args);
            }
            return (object)null;
        }

        public static object GetFieldValue(this object obj, string fieldName)
        {
            var memInf = GetFieldInfo(obj, fieldName);

            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));

            if (memInf is PropertyInfo)
                return memInf.As<PropertyInfo>().GetValue(obj, null);

            if (memInf is FieldInfo)
                return memInf.As<FieldInfo>().GetValue(obj);

            throw new Exception();
        }

        public static void SetFieldValue(this object obj, string fieldName, object newValue)
        {
            var memInf = GetFieldInfo(obj, fieldName);
            
            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));

            if (memInf is PropertyInfo)
                memInf.As<PropertyInfo>().SetValue(obj, newValue, null);
            else if (memInf is FieldInfo)
                memInf.As<FieldInfo>().SetValue(obj, newValue);
            else
                throw new Exception();
        }

        static MethodInfo GetMethodInfo(IReflect classType, string methodName)
        {
            return classType.GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static);
        }

        static MethodInfo GetMethodInfo(object obj, string methodName)
        {
            return GetMethodInfo(obj.GetType(), methodName);
        }

        static MemberInfo GetFieldInfo(IReflect objType, string fieldName)
        {
            var prps = new List<PropertyInfo>();

            prps.Add(objType.GetProperty(fieldName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static));

            prps = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(prps, i => !ReferenceEquals(i, null)));

            if (prps.Count != 0)
                return prps[0];

            var flds = new List<FieldInfo>();

            flds.Add(objType.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static));          

            flds = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(flds, i => !ReferenceEquals(i, null)));

            return flds.Count != 0 ? flds[0] : null;

        }

        static MemberInfo GetFieldInfo(object obj, string fieldName)
        {
            return GetFieldInfo(obj.GetType(), fieldName);
        }

        //Static
        public static void CallStaticMethod(Type classType, string methodName, params object[] args)
        {
            var metInf = GetMethodInfo(classType, methodName);

            if (metInf == null)
                throw new Exception(String.Format("Couldn't find method '{0}' using reflection.", methodName));

            if (metInf != null) {
                MethodInfo meta = metInf.As<MethodInfo>();
                meta.Invoke(null, args);
            }
        }

        public static object GetStaticFieldValue(Type classType, string fieldName)
        {
            var memInf = GetFieldInfo(classType, fieldName);

            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));

            if (memInf is PropertyInfo)
                return memInf.As<PropertyInfo>().GetValue(null, null);

            if (memInf is FieldInfo)
                return memInf.As<FieldInfo>().GetValue(null);

            throw new Exception();
        }

        public static void SetFieldValueValue(Type classType, string fieldName, object newValue)
        {
            var memInf = GetFieldInfo(classType, fieldName);

            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));           

            if (memInf is PropertyInfo)
                memInf.As<PropertyInfo>().SetValue(null, newValue, null);
            else if (memInf is FieldInfo)
                memInf.As<FieldInfo>().SetValue(null, newValue);
            else
                throw new Exception();
        }

        [System.Diagnostics.DebuggerHidden]
        static T As<T>(this object obj)
        {
            return (T)obj;
        }
    }
}
