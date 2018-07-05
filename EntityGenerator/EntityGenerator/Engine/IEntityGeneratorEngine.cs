using CodeGenerators.CodeFirst;

namespace EntityGenerator.Engine
{
	public interface IEntityGeneratorEngine
	{
		string GeneratePocoEntity( string @namespace, string tableName, string entityName, bool makePartialClasses, CodeFirstGenerator generator );
		string GenerateMapping( string @namespace, string tableName, string entityName, CodeFirstGenerator generator );
	}
}