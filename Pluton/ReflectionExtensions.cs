using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Pluton
{   
    public static class ReflectionExtensions
    {  
        public static void CallMethod(this object obj, string methodName, params object[] args)
        {
            var metInf = GetMethodInfo(obj, methodName);

            if (metInf == null)
                throw new Exception(String.Format("Couldn't find method '{0}' using reflection.", methodName));

            if (metInf is MethodInfo)
            {
                MethodInfo meta = metInf.As<MethodInfo>();
                meta.Invoke(obj, args);
            }
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

        public static object SetFieldValue(this object obj, string fieldName, object newValue)
        {
            var memInf = GetFieldInfo(obj, fieldName);
            
            if (memInf == null)
                throw new Exception(String.Format("Couldn't find field '{0}' using reflection.", fieldName));

            var oldValue = obj.GetFieldValue(fieldName);

            if (memInf is System.Reflection.PropertyInfo)
                memInf.As<System.Reflection.PropertyInfo>().SetValue(obj, newValue, null);
            else if (memInf is System.Reflection.FieldInfo)
                memInf.As<System.Reflection.FieldInfo>().SetValue(obj, newValue);
            else
                throw new System.Exception();

            return oldValue;
        }

        private static System.Reflection.MethodInfo GetMethodInfo(object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName,
                                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.FlattenHierarchy);
        }

        private static System.Reflection.MemberInfo GetFieldInfo(object obj, string fieldName)
        {
            var prps = new List<System.Reflection.PropertyInfo>();

            prps.Add(obj.GetType().GetProperty(fieldName,
                                               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                                               System.Reflection.BindingFlags.FlattenHierarchy));

            prps = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(prps, i => !ReferenceEquals(i, null)));

            if (prps.Count != 0)
                return prps[0];

            var flds = new System.Collections.Generic.List<System.Reflection.FieldInfo>();

            flds.Add(obj.GetType().GetField(fieldName,
                                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.FlattenHierarchy));          

            flds = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(flds, i => !ReferenceEquals(i, null)));

            if (flds.Count != 0)
                return flds[0];

            return null;
        }

        [System.Diagnostics.DebuggerHidden]
        private static T As<T>(this object obj)
        {
            return (T)obj;
        }
    }
}
