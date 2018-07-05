namespace CodeGenerators.CodeFirst
{
	public class CollectionNavigationProperty : IGeneratedProperty
	{
		#region CollectionNavigationProperty Members

		public string PropertyType 
		{
			get;
			set;
		}

		public string PropertyName 
		{
			get;
			set;
		}

		public RelationshipType RelationshipType 
		{
			get;
			set;
		}

		public string ReversePropertyName 
		{
			get;
			set;
		}

		public bool IsNullable 
		{
			get;
			set;
		}

		#endregion CollectionNavigationProperty Members

	}
}