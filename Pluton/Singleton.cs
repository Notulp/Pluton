using System;

namespace Pluton
{
    public abstract class Singleton<T> : CountedInstance where T : ISingleton
    {
        protected static T Instance;

        public static T GetInstance()
        {
            return Singleton<T>.Instance;
        }

        static Singleton()
        {
            Singleton<T>.Instance = Activator.CreateInstance<T>();
            if (Singleton<T>.Instance.CheckDependencies())
                Singleton<T>.Instance.Initialize();
            else
                UnityEngine.Debug.LogWarning(String.Format("Couldn't initialite Singleton<{0}>, is one of it's dependencies missing?", Instance.GetType()));
        }
    }
}

