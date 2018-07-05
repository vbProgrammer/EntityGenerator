namespace CodeGenerators.CodeFirst
{
	public class ForeignKeyMapping
	{
		#region ForeignKeyMapping Members

		public string ForeignKeyColumnName
		{
			get;
			set;
		}

		public bool IsForeignKeyNullable
		{
			get;
			set;
		}

		public string PrimaryKeyColumnName
		{
			get;
			set;
		}

		#endregion ForeignKeyMapping Members

	}
}
