using System;

namespace Pluton
{
    public static class SingletonEx
    {
        public static bool IsInitialzed<T>() where T : ISingleton
        {
            return Singleton<T>.GetInstance() != null;
        }
    }
}

