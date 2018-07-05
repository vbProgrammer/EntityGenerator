using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace EntityGenerator.ResourceAccess
{
	public class SqlSchemaResourceAccess : ISqlSchemaResourceAccess
	{
		public string[] GetDatabaseTables( string connectionString, string databaseName )
		{
			string[] result = null;

			using( var connection = new SqlConnection( connectionString ) )
			{
				var connectionInfo = new ServerConnection(connection);
				var server = new Microsoft.SqlServer.Management.Smo.Server( connectionInfo );

				var db = server.Databases[databaseName];
				var tables = db.Tables.Cast<Table>().Select( smoTable => smoTable.Name ).ToArray();

				var views = db.Views.Cast<View>().Select( smoView => smoView.Name ).ToArray();

				result = tables.Union( views ).ToArray();
			}

			return result;
		}
	}
}
