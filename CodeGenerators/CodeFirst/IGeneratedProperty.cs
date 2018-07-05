namespace CodeGenerators.CodeFirst
{
	public interface IGeneratedProperty
	{
		#region IGeneratedProperty Members

		string PropertyName
		{
			get;
			set;
		}

		string PropertyType
		{
			get;
			set;
		}

		#endregion IGeneratedProperty Members

	}
}
