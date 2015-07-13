using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;

namespace Pluton.Patcher
{
	static class Empty<T>
	{
		public static readonly T[] Array = new T [0];
	}

	static class TypeDefinitionExtensions
	{
		public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition self)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			return !self.HasMethods ? Empty<MethodDefinition>.Array : self.Methods.Where(method => method.IsConstructor);

		}

		public static MethodDefinition GetStaticConstructor(this TypeDefinition self)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			return !self.HasMethods ? null : self.GetConstructors().FirstOrDefault(ctor => ctor.IsStatic);

		}

		public static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition self)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			return !self.HasMethods ? Empty<MethodDefinition>.Array : self.Methods.Where(method => !method.IsConstructor);

		}

		public static MethodDefinition GetMethod(this TypeDefinition self, String name)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			return !self.HasMethods ? null : self.Methods.FirstOrDefault(v => v.Name == name);

		}

		public static FieldDefinition GetField(this TypeDefinition self, String name)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			return !self.HasFields ? null : self.Fields.FirstOrDefault(v => v.Name == name);

		}

		public static PropertyDefinition GetProperty(this TypeDefinition self, String name)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			return !self.HasProperties ? null : self.Properties.FirstOrDefault(v => v.Name == name);

		}

		public static TypeDefinition GetNestedType(this TypeDefinition self, String name)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			return !self.HasNestedTypes ? null : self.NestedTypes.FirstOrDefault(v => v.Name == name);

		}
	}
}