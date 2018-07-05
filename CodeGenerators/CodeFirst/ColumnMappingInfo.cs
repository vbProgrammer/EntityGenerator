namespace CodeGenerators.CodeFirst
{
	public class ColumnMappingInfo
	{
		#region ColumnMappingInfo Members

		public string ColumnName
		{
			get;
			set;
		}

		public bool IsFixedLength
		{
			get;
			set;
		}

		public bool IsNullable
		{
			get;
			set;
		}

		public bool IsPrimaryKey
		{
			get;
			set;
		}

		public bool IsRowVersion
		{
			get;
			set;
		}

		public bool IsStorageGenerated
		{
			get;
			set;
		}

		public bool IsUnicode
		{
			get;
			set;
		}

		public int? MaxLength
		{
			get;
			set;
		}

		public byte? Precision
		{
			get;
			set;
		}

		public int? Scale
		{
			get;
			set;
		}

		public string TableName
		{
			get;
			set;
		}

		public string Type
		{
			get;
			set;
		}

		#endregion ColumnMappingInfo Members

	}
}