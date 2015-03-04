using System;

namespace Pluton
{
    public abstract class Singleton<T> : CountedInstance where T : ISingleton
    {
        static T Instance;

        public static T GetInstance()
        {
            return Singleton<T>.Instance;
        }

        static Singleton()
        {
            Singleton<T>.Instance = Activator.CreateInstance<T>();
            Singleton<T>.Instance.Initialize();
        }
    }
}

