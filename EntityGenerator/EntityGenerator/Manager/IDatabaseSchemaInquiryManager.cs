using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityGenerator.Manager
{
	public interface IDatabaseSchemaInquiryManager
	{
		Task<IEnumerable<string>> RetrieveDatabaseTables( string server, string database, string username, string password );
	}
}
