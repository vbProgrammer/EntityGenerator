using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerators.CodeFirst;

namespace EntityGenerator.Engine
{
	public partial class PocoClassTemplate
	{
		#region PocoClassTemplate Members

		public string GeneratePoco( string @namespace, string tbl, string al, bool part, CodeFirstGenerator gen )
		{
			mNamespace = @namespace;
			mTableName = tbl;
			mTableAlias = al;
			mMakePartialClasses = part;
			mGenerator = gen;

			return TransformText();
		}

		#endregion PocoClassTemplate Members

		#region Fields

		private bool mMakePartialClasses;
		private CodeFirstGenerator mGenerator;
		private string mNamespace;
		private string mTableName;
		private string mTableAlias;

		#endregion Fields

	}
}
