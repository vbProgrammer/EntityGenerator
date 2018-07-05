using System.Collections.Generic;

namespace CodeGenerators.CodeFirst
{
	public class ManyManyRelationship
	{
		#region ManyManyRelationship Members

		public IEnumerable<string> LeftForeignKeyColumnNames
		{
			get;
			set;
		}

		public string LeftPluralizedTableName
		{
			get;
			set;
		}

		public string LeftTableName
		{
			get;
			set;
		}

		public string ManyToManyTableName
		{
			get;
			set;
		}

		public IEnumerable<string> RightForeignKeyColumnNames
		{
			get;
			set;
		}

		public string RightPluralizedTableName
		{
			get;
			set;
		}

		public string RightTableName
		{
			get;
			set;
		}

		#endregion ManyManyRelationship Members

	}
}