using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerators.CodeFirst;

namespace EntityGenerator.Engine
{
	public partial class MappingClassTemplate
	{
		#region MappingClassTemplate Members

		public string GenerateMappingClass( string @namespace, string tbl, string al, CodeFirstGenerator gen )
		{
			mNamespace = @namespace;
			mTableName = tbl;
			mTableAlias = al;
			mGenerator = gen;

			return TransformText();
		}

		#endregion MappingClassTemplate Members

		#region Fields
		
		private CodeFirstGenerator mGenerator;
		private string mNamespace;
		private string mTableName;
		private string mTableAlias;

		#endregion Fields

	}
}
