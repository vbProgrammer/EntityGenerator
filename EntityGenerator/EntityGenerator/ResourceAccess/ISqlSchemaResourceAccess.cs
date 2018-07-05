using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityGenerator.ResourceAccess
{
	public interface ISqlSchemaResourceAccess
	{
		string[] GetDatabaseTables( string connectionString, string databaseName );
	}
}
