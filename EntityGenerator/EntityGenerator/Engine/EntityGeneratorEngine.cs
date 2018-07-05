using System;
using CodeGenerators.CodeFirst;

namespace EntityGenerator.Engine
{
	public class EntityGeneratorEngine : IEntityGeneratorEngine
	{
		#region EntityGeneratorEngine Members

		public string GenerateMapping( string @namespace, string tableName, string entityName, CodeFirstGenerator generator )
		{
			var template = new MappingClassTemplate();
			return template.GenerateMappingClass( @namespace, tableName, entityName, generator );
		}

		public string GeneratePocoEntity( string @namespace, string tableName, string entityName, bool makePartialClasses, CodeFirstGenerator generator )
		{
			PocoClassTemplate template = new PocoClassTemplate();
			return template.GeneratePoco( @namespace, tableName, entityName, makePartialClasses, generator );
		}

		#endregion EntityGeneratorEngine Members

	}
}
