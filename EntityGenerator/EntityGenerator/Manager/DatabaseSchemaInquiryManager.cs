using System.Collections.Generic;
using System.Threading.Tasks;

using EntityGenerator.ResourceAccess;

namespace EntityGenerator.Manager
{
	public class DatabaseSchemaInquiryManager : IDatabaseSchemaInquiryManager
	{
		#region DatabaseSchemaInquiryManager Members

		public async Task<IEnumerable<string>> RetrieveDatabaseTables( string server, string database, string username, string password )
		{
			var connectionString = string.Format( @"Data Source={0};Initial Catalog={1};User Id={2};Password={3};", server, database, username, password );

			return SchemaResourceAccess.GetDatabaseTables( connectionString, database );
		}

		#endregion DatabaseSchemaInquiryManager Members

		#region Protected Members

		private ISqlSchemaResourceAccess mSchemaResourceAccess;
		protected ISqlSchemaResourceAccess SchemaResourceAccess
		{
			get
			{
				return mSchemaResourceAccess ?? (mSchemaResourceAccess = new SqlSchemaResourceAccess());
			}
		}

		#endregion Protected Members

	}
}
