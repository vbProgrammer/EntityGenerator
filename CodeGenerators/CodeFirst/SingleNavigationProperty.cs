using System.Collections.Generic;

namespace CodeGenerators.CodeFirst
{
	public class SingleNavigationProperty : IGeneratedProperty
	{
		#region SingleNavigationProperty Members

		public IEnumerable<ForeignKeyMapping> ForeignKeyMappings
		{
			get;
			set;
		}

		public bool IsCascadeDelete
		{
			get;
			set;
		}

		public bool IsNullable
		{
			get;
			set;
		}

		public string PropertyName
		{
			get;
			set;
		}

		public string PropertyType
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

		#endregion SingleNavigationProperty Members

	}
}