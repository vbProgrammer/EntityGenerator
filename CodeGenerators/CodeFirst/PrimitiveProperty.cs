using System.Collections.Generic;

namespace CodeGenerators.CodeFirst
{
	public class PrimitiveProperty : IGeneratedProperty
	{
		#region PrimitiveProperty Members

		private List<PrimativeForeignKeyFixup> mForeignKeys;
		public List<PrimativeForeignKeyFixup> ForeignKeys
		{
			get
			{
				return mForeignKeys ?? (mForeignKeys = new List<PrimativeForeignKeyFixup>());
			}
		}

		public bool IsPrimaryKey
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

		#endregion PrimitiveProperty Members

	}
}
