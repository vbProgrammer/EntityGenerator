using EntityGenerator;

using NUnit.Framework;

namespace CodeGeneratorsTests.EntityGeneratorTests
{
	[TestFixture]
	public class FilePathSplitterTests
	{
		#region FilePathSplitterTests Members

		[TestCase( "C:\\Repositories\\Prod\\XXXX\\XXX.Dal\\Foo\\Bar\\Entities","Foo\\Bar\\Entities","C:\\Repositories\\Prod\\XXXX\\XXX.Dal", TestName = "Entity In Dal" )]
		[TestCase( "C:\\Repositories\\Prod\\XXXX\\Tests.Entities", "", "C:\\Repositories\\Prod\\XXXX\\Tests.Entities", TestName = "Entity Not In Dal" )]
		public void DetermineEntityFile(string outputPath, string expectedOutputPathPrefix, string expectedOutputPath)
		{
			var splitFilePath = FilePathSplitter.DetermineEntityFile( outputPath );

			Assert.That( splitFilePath.EntityOutputPathPrefix, Is.EqualTo( expectedOutputPathPrefix ) );
			Assert.That( splitFilePath.EntityOutputPath, Is.EqualTo( expectedOutputPath ) );
		}

		[TestCase( "C:\\Repositories\\Prod\\XXXX\\Server\\XXX.Dal\\Entities", "C:\\Repositories\\Prod\\XXXX\\Server", TestName = "File path in server" )]
		[TestCase( "C:\\Repositories\\Prod\\Client\\CU.UI", null, TestName = "File path not in server" )]
		public void DetermineServerPath(string outPath, string expectedServerpath)
		{
			var serverPath = FilePathSplitter.DetermineServerPath( outPath );
			
			Assert.That( serverPath, Is.EqualTo( expectedServerpath ) );
		}

		#endregion FilePathSplitterTests Members

	}
}
