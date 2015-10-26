using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;

namespace Pluton.Patcher
{
	static class MethodDefinitionExtensions
	{
		public static MethodDefinition SetPublic(this MethodDefinition self, bool value)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			self.IsPublic = value;
			self.IsPrivate = !value;

			return self;
		}

        public static string Print(this MethodDefinition self)
        {
            return self.PrintIL() + Environment.NewLine + self.PrintCSharp();
        }

        public static string PrintIL(this MethodDefinition self)
        {
            try {
                var textoutput = new ICSharpCode.Decompiler.PlainTextOutput();
                var options = new ICSharpCode.ILSpy.DecompilationOptions();
                var lang = new ICSharpCode.ILSpy.ILLanguage(true);
                lang.DecompileMethod(self, textoutput, options);
                return textoutput.ToString();
            } catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                return ex.Message;
            }
        }

        public static string PrintCSharp(this MethodDefinition self)
        {
            try {
                var textoutput = new ICSharpCode.Decompiler.PlainTextOutput();
                var options = new ICSharpCode.ILSpy.DecompilationOptions();
                var lang = new ICSharpCode.ILSpy.CSharpLanguage();
                lang.DecompileMethod(self, textoutput, options);
                return textoutput.ToString();
            } catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                return ex.Message;
            }
        }
	}
}