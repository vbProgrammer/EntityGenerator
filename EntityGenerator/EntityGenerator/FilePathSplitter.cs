using System;
using System.IO;
using System.Linq;

using EntityGenerator.Contracts;

namespace EntityGenerator
{
	public static class FilePathSplitter
	{
		#region FilePathSplitter Members

		public static EntityFile DetermineEntityFile( string outputPath )
		{
			string entityOutputPath = outputPath;
			string entityOutputPathPrefix = string.Empty;

			if( outputPath.Contains( "XXX.Dal" ) )
			{
				var pathParts = outputPath.Split( Path.DirectorySeparatorChar );

				var splitIndex = Array.IndexOf( pathParts, "XXX.Dal" );
				entityOutputPath = string.Join( Path.DirectorySeparatorChar.ToString(), pathParts.Take( splitIndex + 1 ) );
				entityOutputPathPrefix = string.Join( Path.DirectorySeparatorChar.ToString(), pathParts.Skip( splitIndex + 1 ) );
			}

			return new EntityFile
			{
				EntityOutputPath = entityOutputPath,
				EntityOutputPathPrefix = entityOutputPathPrefix
			};
		}

		public static string DetermineServerPath( string outputPath )
		{
			string serverPath = null;

			if( outputPath.Contains( "Server" ) )
			{
				var pathParts = outputPath.Split( Path.DirectorySeparatorChar );

				var splitIndex = Array.IndexOf( pathParts, "Server" );
				serverPath = string.Join( Path.DirectorySeparatorChar.ToString(), pathParts.Take( splitIndex + 1 ) );
			}

			return serverPath;
		}

		#endregion FilePathSplitter Members

	}
}
