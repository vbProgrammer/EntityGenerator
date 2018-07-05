namespace EntityGenerator.Engine
{
	public partial class PrimitivePropertyTemplate
	{
		#region PrimitivePropertyTemplate Members

		public string GenerateProperty( string type, string name )
		{
			mType = type;
			mPropertyName = name;

			// To do this we need to also fix all of the "Map" T4 "ColumnName" stuff to also fix "ID" -> "Id"
			//if( name.EndsWith( "ID" ) )
			//{
			//	mPropertyName = mPropertyName.Substring( 0, mPropertyName.Length - 1 ) + "d";
			//}

			return TransformText();
		}

		#endregion PrimitivePropertyTemplate Members

		#region Fields

		private string mType;
		private string mPropertyName;

		#endregion Fields

	}
}
