using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;

namespace Pluton.Patcher
{
	static class FieldDefinitionExtensions
	{
		public static FieldDefinition SetPublic(this FieldDefinition self, bool value)
		{
			if (self == null) {
				throw new ArgumentNullException("self");
			}

			self.IsPublic = value;
			self.IsPrivate = !value;

			return self;
		}
	}
}