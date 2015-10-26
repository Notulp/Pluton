using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Pluton.Patcher.Reflection
{
    public class MethodPatcher : PatcherObject
    {
        internal MethodDefinition methodDefinition;
        internal ILProcessor IlProc;

        string original;

        public MethodPatcher(PatcherObject prnt, MethodDefinition metDef) : base(prnt)
        {
            methodDefinition = metDef;
            IlProc = metDef.Body.GetILProcessor();
            rootAssemblyPatcher = prnt.rootAssemblyPatcher;

            if (MainClass.gendiffs && MainClass.newAssCS)
                original = metDef.Print();
        }

        public MethodPatcher Clear()
        {
            methodDefinition.Body.Instructions.Clear();
            methodDefinition.Body.ExceptionHandlers.Clear();
            methodDefinition.Body.Variables.Clear();
            return this;
        }

        public MethodPatcher Append(Instruction i)
        {
            IlProc.Append(i);
            return this;
        }

        public MethodPatcher AppendCall(MethodPatcher method)
        {
            return Append(Instruction.Create(OpCodes.Call, rootAssemblyPatcher.ImportMethod(method)));
        }

        public MethodPatcher InsertAfter(Instruction after, Instruction insert)
        {
            IlProc.InsertAfter(after, insert);
            return this;
        }

        public MethodPatcher InsertAfter(int after, Instruction insert)
        {
            return InsertAfter(IlProc.Body.Instructions[after], insert);
        }

        public MethodPatcher InsertCallAfter(Instruction after, MethodPatcher method)
        {
            return InsertAfter(after, Instruction.Create(OpCodes.Call, rootAssemblyPatcher.ImportMethod(method)));
        }

        public MethodPatcher InsertCallAfter(int after, MethodPatcher method)
        {
            return InsertCallAfter(IlProc.Body.Instructions[after], method);
        }

        public MethodPatcher InsertBefore(Instruction before, Instruction insert)
        {
            IlProc.InsertBefore(before, insert);
            return this;
        }

        public MethodPatcher InsertBefore(int before, Instruction insert)
        {
            return InsertBefore(IlProc.Body.Instructions[before], insert);
        }

        public MethodPatcher InsertBeforeRet(Instruction insert)
        {
            return InsertBefore(IlProc.Body.Instructions[IlProc.Body.Instructions.Count - 1], insert);
        }

        public MethodPatcher InsertCallBeforeRet(MethodPatcher method)
        {
            return InsertBefore(IlProc.Body.Instructions[IlProc.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Call, rootAssemblyPatcher.ImportMethod(method)));
        }

        public MethodPatcher InsertCallBefore(Instruction before, MethodPatcher method)
        {
            return InsertBefore(before, Instruction.Create(OpCodes.Call, rootAssemblyPatcher.ImportMethod(method)));
        }

        public MethodPatcher InsertCallBefore(int before, MethodPatcher method)
        {
            return InsertCallBefore(IlProc.Body.Instructions[before], method);
        }

        public MethodPatcher RemoveAt(int rem)
        {
            IlProc.Remove(IlProc.Body.Instructions[rem]);
            return this;
        }

        public MethodPatcher RemoveRange(int from, int to)
        {
            for (int i = to; i >= from; i--)
                IlProc.Body.Instructions.RemoveAt(i);
            return this;
        }

        public string FriendlyName {
            get {
                return String.Format("{0}.{1}",
                    parent.As<TypePatcher>().typeDefinition.Name,
                    methodDefinition.Name);
            }
        }

        public string IlName {
            get {
                return String.Format("{0}.{1}::{2}",
                    parent.As<TypePatcher>().typeDefinition.Namespace,
                    parent.As<TypePatcher>().typeDefinition.Name,
                    methodDefinition.Name);
            }
        }

        public string PrintAndLink(params MethodPatcher[] others)
        {
            string diffHtml   = MainClass.GetHtmlDiff(original, ToString()),
                   otherPrint = String.Empty;

            foreach (MethodPatcher other in others) {
                otherPrint = other.methodDefinition.PrintCSharp().Replace("\"", "\\'").TrimEnd('\\').Replace("\r\n", ".enter.").Replace("\t", ".tab.").Replace("//", "##");
                diffHtml = diffHtml
                    .Replace(other.FriendlyName, "<a href=\"javascript:void(0);\" onclick=\"hatemlPopUp('" + otherPrint + "')\">" + other.FriendlyName + "</a>")
                    .Replace(other.IlName,       "<a href=\"javascript:void(0);\" onclick=\"hatemlPopUp('" + otherPrint + "')\">" + other.IlName       + "</a>");
            }
            return diffHtml;
        }

        public override string ToString()
        {
            return methodDefinition.Print();
        }
    }
}

