using System;

namespace Pluton.Patcher.Reflection
{
    public class PatcherObject
    {
        protected PatcherObject parent;
        internal AssemblyPatcher rootAssemblyPatcher;

        public PatcherObject(){}

        public PatcherObject(PatcherObject prnt)
        {
            parent = prnt;
        }

        public T As<T> () where T : PatcherObject
        {
            return (T)this;
        }
    }
}

