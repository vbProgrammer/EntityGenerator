using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;

using EntityGenerator.Contracts;
using EntityGenerator.Manager;

using Microsoft.Practices.Prism.Commands;

using MessageBox = System.Windows.Forms.MessageBox;

namespace EntityGenerator.UI
{
	public class EntityEntryViewModel : INotifyPropertyChanged
	{
		#region Constructors

		public EntityEntryViewModel( SelectedTablesPayload payload )
		{
			mPayload = payload;

			CandidateDefinitions = payload.SelectedTables.Select( table => new EntityDefinition()
			{
				TableName = table,
				EntityName = table + "Entity"
			} ).ToArray();

			if( payload.FilePath != null )
			{
				OutputPath = payload.FilePath;
			}

			else if( !string.IsNullOrEmpty( Properties.Settings.Default.LastOutputPath ) )
			{
				OutputPath = Properties.Settings.Default.LastOutputPath;
			}
			else
			{
				var currentDirectory = Directory.GetCurrentDirectory();
				if( currentDirectory.EndsWith( @"XXX\Tools\CodeGenerators\EntityGenerator\EntityGenerator\bin\Debug" ) )
				{
					OutputPath = currentDirectory.Replace( @"XXX\Tools\CodeGenerators\EntityGenerator\EntityGenerator\bin\Debug", @"XXX\Server" );
				}
				else
				{
					OutputPath = currentDirectory;
				}
			}

			if( payload.FilePath == null )
			{
				if( !string.IsNullOrEmpty( Properties.Settings.Default.LastNamespace ) )
				{
					Namespace = Properties.Settings.Default.LastNamespace;
				}
				else
				{
					Namespace = "XXX";
				}
			}

			if( !string.IsNullOrEmpty( Properties.Settings.Default.LastEntityNames ) )
			{
				var oldPairs = Properties.Settings.Default.LastEntityNames.Split( '@' );
				foreach( var oldPair in oldPairs )
				{
					var tablePairs = oldPair.Split( '#' );
					var newDef = CandidateDefinitions.SingleOrDefault( def => def.TableName == tablePairs[0] );
					if( newDef != null )
					{
						newDef.EntityName = tablePairs[1];
					}
				}
			}
		}

		#endregion Constructors

		#region EntityEntryViewModel Members

		private ICommand mBackCommand;
		public ICommand BackCommand
		{
			get
			{
				return mBackCommand ?? (mBackCommand = new DelegateCommand( _HandleBackCommandExecute ));
			}
		}

		public Visibility ExitButtonVisibility
		{
			get
			{
				var exitButtonVisibility = Visibility.Visible;
				if( !EntityGeneratorViewModel.CanExit() )
				{
					exitButtonVisibility = Visibility.Collapsed;
				}

				return exitButtonVisibility;
			}
		}

		private ICommand mExitCommand;
		public ICommand ExitCommand
		{
			get
			{
				return mExitCommand ?? (mExitCommand = new DelegateCommand( _HandleExitCommandExecute ));
			}
		}

		private IEnumerable<EntityDefinition> mCandidateDefinitions;
		public IEnumerable<EntityDefinition> CandidateDefinitions
		{
			get
			{
				return mCandidateDefinitions;
			}
			set
			{
				mCandidateDefinitions = value;
				OnPropertyChanged();
			}
		}

		private DelegateCommand mGenerateCodeCommand;
		public DelegateCommand GenerateCodeCommand
		{
			get
			{
				return mGenerateCodeCommand ?? (mGenerateCodeCommand = new DelegateCommand( _HandleGenerateCodeCommandExecuted ));
			}
		}

		private string mNamespace;
		public string Namespace
		{
			get
			{
				return mNamespace;
			}
			set
			{
				mNamespace = value;
				OnPropertyChanged();
			}
		}

		private string mOutputPath;
		public string OutputPath
		{
			get
			{
				return mOutputPath;
			}
			set
			{
				mOutputPath = value;
				OnPropertyChanged();

				_GuessNamespace();
			}
		}

		private DelegateCommand mSelectOutputPathCommand;
		public DelegateCommand SelectOutputPathCommand
		{
			get
			{
				return mSelectOutputPathCommand ?? (mSelectOutputPathCommand = new DelegateCommand( _HandleSelectOutputPathCommandExecuted ));
			}
		}

		#endregion EntityEntryViewModel Members

		#region Ëvents

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Ëvents

		#region Fields

		private SelectedTablesPayload mPayload;

		#endregion Fields

		#region Private Members

		private void _GuessNamespace()
		{
			if( OutputPath.EndsWith( ".Entities" ) )
			{
				// Likely in an RA component, try to grab namespace from CSPROJ
				var projectFiles = Directory.GetFiles( OutputPath, "*.Entities.csproj" );
				if( projectFiles.Length == 1 )
				{
					XDocument docment = XDocument.Load( projectFiles[0] );
					var projectDefaultNamespace = docment.Descendants().SingleOrDefault( element => element.Name.LocalName.EndsWith( "RootNamespace" ) );
					if( projectDefaultNamespace != null )
					{
						Namespace = projectDefaultNamespace.Value;
					}
				}
			}
			else if( OutputPath.Contains( "XXX.Dal" ) )
			{
				// Derive namespace from folders

				var pathParts = OutputPath.Split( '\\' );
				var namespacePart = pathParts.Last();
				if( namespacePart == "Entities" )
				{
					namespacePart = pathParts.Reverse().Skip( 1 ).First();
				}

				Namespace = "XXX." + namespacePart;
			}
			else if( OutputPath.Contains( "XXX.Models" ) )
			{
				MessageBox.Show( "Entities should NEVER be placed in XXX.Models.",
					"ENTITIES NEVER GO IN MODELS", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void _HandleBackCommandExecute()
		{
			EntityGeneratorViewModel.PopViewModel();
		}

		private void _HandleExitCommandExecute()
		{
			EntityGeneratorViewModel.Exit();
		}

		private async void _HandleGenerateCodeCommandExecuted()
		{
			if( !CandidateDefinitions.Any( def => string.IsNullOrEmpty( def.TableName ) ) )
			{
				EntityGeneratorViewModel.ShowBusy( "Generating entities..." );

				var request = new EntityGenerationRequest
				{
					OutputPath = OutputPath,
					Server = mPayload.Server,
					Database = mPayload.Database,
					Username = mPayload.Username,
					Password = mPayload.Password,
					EntityDefinitions = CandidateDefinitions.ToArray(),
					Namespace = Namespace,
				};

				await EntityGenerationManager.GenerateEntities( request );

				Properties.Settings.Default.LastEntityNames = string.Join( "@", request.EntityDefinitions.Select( entity => string.Format( "{0}#{1}", entity.TableName, entity.EntityName ) ) );
				Properties.Settings.Default.LastNamespace = Namespace;
				Properties.Settings.Default.LastOutputPath = OutputPath;
				Properties.Settings.Default.Save();

				EntityGeneratorViewModel.HideBusy();

				MessageBox.Show(
				@"Generation complete! ", "Entity Generation Complete", MessageBoxButtons.OK, MessageBoxIcon.Asterisk );
			}
			else
			{
				MessageBox.Show( "An entity name is required for all tables. Please define an entity name for the following tables: " +
					string.Join( ",", CandidateDefinitions.Where( def => string.IsNullOrEmpty( def.EntityName ) ).Select( def => def.TableName ) ) );
			}
		}

		private void _HandleSelectOutputPathCommandExecuted()
		{
			using( var picker = new FolderBrowserDialog() )
			{
				picker.SelectedPath = OutputPath;
				var result = picker.ShowDialog();

				if( result == DialogResult.OK )
				{
					OutputPath = picker.SelectedPath;
				}
			}
		}

		#endregion Private Members

		#region Protected Members

		private IEntityGenerationManager mEntityGenerationManager;
		protected IEntityGenerationManager EntityGenerationManager
		{
			get
			{
				return mEntityGenerationManager ?? (mEntityGenerationManager = new EntityGenerationManager());
			}
		}

		protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion Protected Members

	}
}
