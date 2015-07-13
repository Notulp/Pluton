using System;
using System.Collections.Generic;

namespace Pluton
{
    [Serializable]
    public class CountedInstance
    {
        [NonSerialized]
        public static Dictionary<Type, CountedInstance.Counts> InstanceTypes = new Dictionary<Type, CountedInstance.Counts>();

        ~CountedInstance()
        {
            CountedInstance.RemoveCount(GetType());
        }

        public CountedInstance()
        {
            CountedInstance.AddCount(GetType());
        }

        static CountedInstance()
        {
            CountedInstance.InstanceTypes = new Dictionary<Type, CountedInstance.Counts>();
        }

        internal static void AddCount(Type type)
        {
            CountedInstance.Counts counts;
            if (CountedInstance.InstanceTypes.TryGetValue(type, out counts)) {
                counts.Created++;
                return;
            }
            CountedInstance.InstanceTypes.Add(type, new CountedInstance.Counts());
        }

        internal static void RemoveCount(Type type)
        {
            CountedInstance.Counts counts;
            if (CountedInstance.InstanceTypes.TryGetValue(type, out counts)) {
                counts.Destroyed++;
            }
        }

        public static string InstanceReportText()
        {
            string text = "";
            foreach (KeyValuePair<Type, CountedInstance.Counts> current in CountedInstance.InstanceTypes) {
                object obj = text;
                text = String.Concat(new object[] {
                    obj,
                    current.Key.FullName,
                    Environment.NewLine + "\tCurrently:\t",
                    current.Value.Created - current.Value.Destroyed,
                    Environment.NewLine + "\tCreated:  \t",
                    current.Value.Created,
                    Environment.NewLine
                });
            }
            return text;
        }

        [Serializable]
        public class Counts
        {
            public int Created = 1;
            public int Destroyed;
        }
    }
}

