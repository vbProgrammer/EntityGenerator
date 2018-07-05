
namespace EntityGenerator.Contracts
{
	public class EntityGenerationRequest
	{
		#region EntityGenerationRequest Members

		public string Database
		{
			get;
			set;
		}

		public string Namespace
		{
			get;
			set;
		}

		public EntityDefinition[] EntityDefinitions
		{
			get;
			set;
		}

		public string OutputPath
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		public string Server
		{
			get;
			set;
		}

		public string Username
		{
			get;
			set;
		}

		#endregion EntityGenerationRequest Members

	}
}
