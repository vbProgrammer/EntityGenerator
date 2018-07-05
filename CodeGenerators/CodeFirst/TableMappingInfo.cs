namespace CodeGenerators.CodeFirst
{
	public class TableMappingInfo
	{
		#region TableMappingInfo Members

		public string FKName
		{
			get;
			set;
		}

		public bool IsCascadeDelete
		{
			get;
			set;
		}

		public bool OneToOneRelationship
		{
			get;
			set;
		}

		public string ReferenceTable
		{
			get;
			set;
		}

		public string SourceTable
		{
			get;
			set;
		}

		#endregion TableMappingInfo Members

	}
}