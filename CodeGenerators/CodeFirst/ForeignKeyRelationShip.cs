using System.Collections.Generic;

namespace CodeGenerators.CodeFirst
{
	public class ForeignKeyRelationship
	{
		#region ForeignKeyRelationship Members

		public bool IsForeignKeyOptional
		{
			get;
			set;
		}

		public string ReferenceTableName
		{
			get;
			set;
		}

		public RelationshipType RelationshipType
		{
			get;
			set;
		}

		public IEnumerable<string> SourceForeignKeyColumnNames
		{
			get;
			set;
		}

		public string SourcePluralizedTableName
		{
			get;
			set;
		}

		public string SourceTableName
		{
			get;
			set;
		}

		#endregion ForeignKeyRelationship Members

	}
}