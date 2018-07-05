namespace CodeGenerators.CodeFirst
{
	public class ColumnAssociationInfo
	{
		#region ColumnMappingInfo Members

		public string DeleteRule
		{
			get;
			set;
		}

		public string ForeignKey
		{
			get;
			set;
		}

		public string ForeignKeyFrom
		{
			get;
			set;
		}

		public string ForeignKeyFromColumn
		{
			get;
			set;
		}

		public string ForeignKeyTo
		{
			get;
			set;
		}

		public string ForeignKeyToColumn
		{
			get;
			set;
		}

		public bool IsOneToOne
		{
			get;
			set;
		}

		#endregion ColumnMappingInfo Members

	}
}