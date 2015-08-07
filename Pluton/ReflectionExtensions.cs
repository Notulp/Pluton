using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if (metInf is MethodInfo) {
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

            if (memInf is System.Reflection.PropertyInfo)
                return memInf.As<System.Reflection.PropertyInfo>().GetValue(obj, null);

            if (memInf is System.Reflection.FieldInfo)
                return memInf.As<System.Reflection.FieldInfo>().GetValue(obj);

            throw new Exception();
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
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static));          

            flds = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(flds, i => !ReferenceEquals(i, null)));

            if (flds.Count != 0)
                return flds[0];

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
