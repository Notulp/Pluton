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

			if (!self.HasMethods) {
				return Empty<MethodDefinition>.Array;
			}

			return self.Methods.Where(method => method.IsConstructor);
		}

		public static MethodDefinition GetStaticConstructor(this TypeDefinition self)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			if (!self.HasMethods) {
				return null;
			}

			return self.GetConstructors().FirstOrDefault(ctor => ctor.IsStatic);
		}

		public static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition self)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			if (!self.HasMethods) {
				return Empty<MethodDefinition>.Array;
			}

			return self.Methods.Where(method => !method.IsConstructor);
		}

		public static MethodDefinition GetMethod(this TypeDefinition self, String name)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			if (!self.HasMethods) {
				return null;
			}

			return self.Methods.FirstOrDefault(v => v.Name == name);
		}

		public static FieldDefinition GetField(this TypeDefinition self, String name)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			if (!self.HasFields) {
				return null;
			}

			return self.Fields.FirstOrDefault(v => v.Name == name);
		}

		public static PropertyDefinition GetProperty(this TypeDefinition self, String name)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			if (!self.HasProperties) {
				return null;
			}

			return self.Properties.FirstOrDefault(v => v.Name == name);
		}

		public static TypeDefinition GetNestedType(this TypeDefinition self, String name)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			if (!self.HasNestedTypes) {
				return null;
			}

			return self.NestedTypes.FirstOrDefault(v => v.Name == name);
		}
	}
}