using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using CodeGenerators.CodeFirst;
using EntityGenerator.Contracts;
using EntityGenerator.Engine;
using EntityGenerator.UI;

using Microsoft.Build.Evaluation;

namespace EntityGenerator.Manager
{
	public class EntityGenerationManager : IEntityGenerationManager
	{
		#region EntityGenerationManager Members

		public async Task GenerateEntities( EntityGenerationRequest request )
		{
			var connectionString = string.Format( @"Data Source={0};Initial Catalog={1};User Id={2};Password={3};",
				request.Server, request.Database, request.Username, request.Password );

			var generator = CodeFirstGenerator.Load( connectionString, request.EntityDefinitions.ToDictionary( def => def.TableName, def => def.EntityName ) );
			bool makePartialClasses = false;

			EntityFile entityFile = FilePathSplitter.DetermineEntityFile( request.OutputPath );

			var projectDirectoryPaths = Directory.GetFiles( entityFile.EntityOutputPath, "*.csproj" );

			if( projectDirectoryPaths.Length == 1 )
			{
				var projectDirectoryPath = projectDirectoryPaths.Single();

				var project = new Project( projectDirectoryPath );

				var serverPath = FilePathSplitter.DetermineServerPath( request.OutputPath );

				foreach( var entityDefinition in request.EntityDefinitions.Where( table => !generator.IsManyToManyMappingTable( table.TableName ) ) )
				{
					var tryToSave = _SearchForExistingTableName( entityDefinition.TableName, serverPath );

					if( tryToSave )
					{
						var pocoEntity = EntityGeneratorEngine.GeneratePocoEntity( request.Namespace, entityDefinition.TableName, entityDefinition.EntityName, makePartialClasses, generator );
						_SaveOutput( request.OutputPath, Path.Combine( entityDefinition.EntityName + ".cs" ), entityFile.EntityOutputPathPrefix, pocoEntity, project );

						var mapping = EntityGeneratorEngine.GenerateMapping( request.Namespace + ".Mapping", entityDefinition.TableName, entityDefinition.EntityName, generator );
						_SaveOutput( request.OutputPath, Path.Combine( "Mapping", entityDefinition.EntityName + "Map.cs" ), entityFile.EntityOutputPathPrefix, mapping, project );
					}
				}

				if( project.IsDirty )
				{
					_SortProjectDirectory( projectDirectoryPath );
				}

				ProjectCollection.GlobalProjectCollection.UnloadProject( project );
			}
		}

		#endregion EntityGenerationManager Members

		#region Fields

		private static XNamespace msbuildNS = @"http://schemas.microsoft.com/developer/msbuild/2003";
		private Window mWindow;

		#endregion Fields

		#region Private Members

		private void _HandleFinished( object sender, EventArgs eventArgs )
		{
			var existingTableMappingsWindowViewModel = (ExistingTableMappingsWindowViewModel)sender;
			mWindow.DialogResult = existingTableMappingsWindowViewModel.OkClicked;
		}

		private void _SaveOutput( string outputPath, string fileName, string fileNamePrefix, string fileContents, Project project )
		{
			var filePath = Path.Combine( outputPath, fileName );
			bool doSave = true;
			bool fileExists = false;
			if( File.Exists( filePath ) )
			{
				fileExists = true;
				doSave = MessageBox.Show(
					string.Format( "The file [{0}] already exists. Would you like to overwrite it?", filePath ),
					"File exists", MessageBoxButton.YesNo ) == MessageBoxResult.Yes;
			}

			if( doSave )
			{
				var directory = Path.GetDirectoryName( filePath );
				if( !Directory.Exists( directory ) )
				{
					Directory.CreateDirectory( directory );
				}

				File.WriteAllText( filePath, fileContents, Encoding.UTF8 );
				if( !fileExists )
				{
					_UpdateProjectFile( Path.Combine( fileNamePrefix, fileName ), project );
				}
			}
		}

		private bool _SearchForExistingTableName( string tableName, string serverPath )
		{
			var tryToSave = true;
			var matchedFileFound = false;

			DirectoryInfo directoryInfo = new DirectoryInfo( serverPath );

			var serverEntityMapFiles = directoryInfo.GetFiles( "*EntityMap.cs", SearchOption.AllDirectories );
			var matchedFiles = new List<FileInfo>();

			var searchText = string.Format( "ToTable\\(\\s*\\\"{0}\\\"\\s*\\)", tableName );

			foreach( var file in serverEntityMapFiles )
			{
				StreamReader streamReader = new StreamReader( file.FullName );
				string fileContents = streamReader.ReadToEnd();
				streamReader.Close();
				if( Regex.IsMatch( fileContents, searchText ) )
				{
					matchedFiles.Add( file );

					matchedFileFound = true;
				}
			}

			if( matchedFileFound )
			{
				var window = new ExistingTableMappingsWindow
				{
					SizeToContent = SizeToContent.WidthAndHeight
				};

				var payload = new ExistingTableMappingsWindowPayload
				{
					MatchedFiles = matchedFiles,
					TableName = tableName
				};
				var vm = new ExistingTableMappingsWindowViewModel( payload );
				vm.Finished += _HandleFinished;
				window.DataContext = vm;
				mWindow = window;
				tryToSave = window.ShowDialog().GetValueOrDefault();
			}

			return tryToSave;
		}

		private void _SortProjectDirectory( string projectDirectoryPath )
		{
			XDocument projXml = XDocument.Load( projectDirectoryPath );

			var itemGroupsWithCompileDirectives = projXml.Descendants( msbuildNS + "Compile" ).Select( compileDirective => compileDirective.Parent ).Distinct().ToArray();

			foreach( var distinctItemGroup in itemGroupsWithCompileDirectives )
			{
				var sortedChildren = distinctItemGroup.Elements().OrderBy( descendant => descendant.ToString() ).ToArray();

				distinctItemGroup.Elements().Remove();
				distinctItemGroup.Add( sortedChildren );
			}

			bool changesMade = projXml.ToString() != XDocument.Load( projectDirectoryPath ).ToString();

			if( changesMade )
			{
				projXml.Save( projectDirectoryPath );
			}
		}

		private void _UpdateProjectFile( string fileName, Project projectDirectory )
		{
			projectDirectory.AddItem( "Compile", fileName );
			projectDirectory.Save();
		}

		#endregion Private Members

		#region Protected Members

		private IEntityGeneratorEngine mEntityGeneratorEngine;
		protected IEntityGeneratorEngine EntityGeneratorEngine
		{

			get
			{
				return mEntityGeneratorEngine ?? (mEntityGeneratorEngine = new EntityGeneratorEngine());
			}
		}

		#endregion Protected Members

	}
}
