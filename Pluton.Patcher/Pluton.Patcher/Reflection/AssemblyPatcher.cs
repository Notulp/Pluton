using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

namespace Pluton.Patcher.Reflection
{
    public class AssemblyPatcher : PatcherObject
    {
        internal AssemblyDefinition assemblyDefinition;
        internal ModuleDefinition mainModule;

        internal Mono.Collections.Generic.Collection<TypeDefinition> Types {
            get {
                return mainModule.Types;
            }
        }

        public AssemblyPatcher(AssemblyDefinition assDef)
        {
            if (assDef == null)
                return;
            
            assemblyDefinition = assDef;
            mainModule = assemblyDefinition.MainModule;
            rootAssemblyPatcher = this;
        }

        public static AssemblyPatcher FromFile(string filename)
        {
            return new AssemblyPatcher(AssemblyDefinition.ReadAssembly(filename));
        }

        public TypePatcher CreateType(string @namespace, string name)
        {
            TypeDefinition plutonClass = new TypeDefinition(@namespace, name, TypeAttributes.Public, mainModule.Import(typeof(Object)));
            mainModule.Types.Add(plutonClass);
            return GetType(name);
        }

        public TypePatcher GetType(string type)
        {
            var t = mainModule.GetType(type);
            if (t == null) return null;

            return new TypePatcher(this, t);
        }

        public TypePatcher GetType(Func<IEnumerable<TypeDefinition>, TypeDefinition> func)
        {
            return new TypePatcher(this, func.Invoke(mainModule.GetTypes()));
        }

        public MethodReference ImportMethod(MethodPatcher toImport)
        {
            return mainModule.Import(toImport.methodDefinition);
        }

        public void Write(string file)
        {
            assemblyDefinition.Write(file);
        }
    }
}

