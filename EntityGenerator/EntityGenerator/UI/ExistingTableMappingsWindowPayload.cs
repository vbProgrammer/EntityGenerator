using System.Collections.Generic;
using System.IO;

namespace EntityGenerator.UI
{
	public class ExistingTableMappingsWindowPayload
	{
		#region ExistingTableMappingsWindowPayload Members

		public List<FileInfo> MatchedFiles
		{
			get;
			set;
		}

		public string TableName
		{
			get;
			set;
		}

		#endregion ExistingTableMappingsWindowPayload Members

	}
}
