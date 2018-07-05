using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

using EntityGenerator.Contracts;
using EntityGenerator.Manager;

using Microsoft.Practices.Prism.Commands;

namespace EntityGenerator.UI
{
	public class TableSelectionViewModel : INotifyPropertyChanged
	{
		#region Constructors

		public TableSelectionViewModel()
		{
			Server = Properties.Settings.Default.DefaultServer;
			Database = Properties.Settings.Default.LastDatabase;
			Username = Properties.Settings.Default.DefaultUsername;
			Password = Properties.Settings.Default.DefaultPassword;
		}

		public TableSelectionViewModel( string filePath )
		{
			Server = Properties.Settings.Default.DefaultServer;
			Username = Properties.Settings.Default.DefaultUsername;
			Password = Properties.Settings.Default.DefaultPassword;
			Database = Properties.Settings.Default.LastDatabase;

			FilePath = filePath;
		}

		#endregion Constructors

		#region TableSelectionViewModel Members

		private CollectionViewSource mCandidateTables;
		public IEnumerable<SelectableItem> CandidateTables
		{
			get;
			set;
		}

		public CollectionViewSource CandidateTablesViewSource
		{
			get
			{
				return mCandidateTables;
			}
			set
			{
				mCandidateTables = value;
				OnPropertyChanged();
			}
		}

		private DelegateCommand mClearSelectedTablesCommand;
		public DelegateCommand ClearSelectedTablesCommand
		{
			get
			{
				return mClearSelectedTablesCommand ?? (mClearSelectedTablesCommand = new DelegateCommand( _HandleClearSelectedCommandExecuted ));
			}
		}

		private DelegateCommand mConnectToDatabaseCommand;
		public DelegateCommand ConnectToDatabaseCommand
		{
			get
			{
				return mConnectToDatabaseCommand ?? (mConnectToDatabaseCommand = new DelegateCommand( _HandleConnectToDatabaseCommand ));
			}
		}

		private string mDatabase;
		public string Database
		{
			get
			{
				return mDatabase;
			}
			set
			{
				mDatabase = value;
				OnPropertyChanged();
			}
		}

		private string mFilePath;
		public string FilePath
		{
			get
			{
				return mFilePath;
			}
			set
			{
				mFilePath = value;
				OnPropertyChanged();
			}
		}

		private DelegateCommand mNextPageCommand;
		public DelegateCommand NextPageCommand
		{
			get
			{
				return mNextPageCommand ?? (mNextPageCommand = new DelegateCommand( _HandleNextPageCommand ));
			}
		}

		private string mPassword;
		public string Password
		{
			get
			{
				return mPassword;
			}
			set
			{
				mPassword = value;
				OnPropertyChanged();
			}
		}

		private string mSearchText;
		public string SearchText
		{
			get
			{
				return mSearchText;
			}
			set
			{
				mSearchText = value;
				OnPropertyChanged();

				_RefreshCandidates();
			}
		}

		private string mServer;
		public string Server
		{
			get
			{
				return mServer;
			}
			set
			{
				mServer = value;
				OnPropertyChanged();
			}
		}

		private string mUsername;
		public string Username
		{
			get
			{
				return mUsername;
			}
			set
			{
				mUsername = value;
				OnPropertyChanged();
			}
		}

		#endregion TableSelectionViewModel Members

		#region Ëvents

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Ëvents

		#region Fields

		#endregion Fields

		#region Private Members

		private void _HandleCandidateTablesFilter( object sender, FilterEventArgs eventArgs )
		{
			var item = (SelectableItem)eventArgs.Item;

			eventArgs.Accepted = string.IsNullOrEmpty( SearchText ) || item.IsSelected || item.Description.IndexOf( SearchText, StringComparison.CurrentCultureIgnoreCase ) >= 0;
		}

		private void _HandleClearSelectedCommandExecuted()
		{
			foreach( var item in CandidateTables )
			{
				item.IsSelected = false;
			}
		}

		private async void _HandleConnectToDatabaseCommand()
		{
			if( !string.IsNullOrWhiteSpace( Server ) &&
				!string.IsNullOrEmpty( Database ) &&
				!string.IsNullOrWhiteSpace( Username ) &&
				!string.IsNullOrWhiteSpace( Password ) )
			{
				EntityGeneratorViewModel.ShowBusy( "Connecting..." );

				var results = await SchemaManager.RetrieveDatabaseTables( Server, Database, Username, Password );

				CandidateTables = results.Select( table => new SelectableItem
				{
					Description = table
				} ).ToArray();

				var lastSelected = Properties.Settings.Default.LastSelectedTables;
				if( !string.IsNullOrEmpty( lastSelected ) )
				{
					var tables = lastSelected.Split( '@' );
					foreach( var table in tables )
					{
						var selectableTableItem = CandidateTables.SingleOrDefault( item => item.Description == table );
						if( selectableTableItem != null )
						{
							selectableTableItem.IsSelected = true;
						}
					}
				}

				CandidateTablesViewSource = new CollectionViewSource() { Source = CandidateTables };
				CandidateTablesViewSource.Filter += _HandleCandidateTablesFilter;

				EntityGeneratorViewModel.HideBusy();
			}
			else
			{
				MessageBox.Show( "Please enter a connection string." );
			}
		}

		private void _HandleNextPageCommand()
		{
			if( CandidateTables != null && CandidateTables.Any( item => item.IsSelected ) )
			{
				var selectedTables = CandidateTables.Where( item => item.IsSelected ).ToArray();
				var payload = new SelectedTablesPayload
				{
					SelectedTables = selectedTables.Select( item => item.Description ).ToArray(),
					Server = Server,
					Database = Database,
					Username = Username,
					Password = Password,
					FilePath = FilePath
				};

				Properties.Settings.Default.LastSelectedTables = string.Join( "@", selectedTables.Select( item => item.Description ).ToArray() );

				Properties.Settings.Default.LastDatabase = Database;
				Properties.Settings.Default.DefaultServer = Server;
				Properties.Settings.Default.DefaultUsername = Username;
				Properties.Settings.Default.DefaultPassword = Password;

				Properties.Settings.Default.Save();

				EntityEntryView view = new EntityEntryView( payload );

				EntityGeneratorViewModel.NavigateTo( view );
			}
			else
			{
				MessageBox.Show( "Please connect and select one or more tables to generate entities for." );
			}
		}

		private void _RefreshCandidates()
		{
			CandidateTablesViewSource.View.Refresh();
		}

		private bool _UseVersion60Defaults( string filePath )
		{
			var useVersion60Defaults = false;

			var assemblyInfoPath = Path.Combine( filePath, "Properties\\AssemblyInfo.cs" );
			if( File.Exists( assemblyInfoPath ) )
			{
				var fileText = File.ReadAllText( assemblyInfoPath );
				if( fileText.Contains( "\"60." ) )
				{
					useVersion60Defaults = true;
				}
			}

			return useVersion60Defaults;
		}

		#endregion Private Members

		#region Protected Members

		private IDatabaseSchemaInquiryManager mSchemaManager;
		protected IDatabaseSchemaInquiryManager SchemaManager
		{
			get
			{
				return mSchemaManager ?? (mSchemaManager = new DatabaseSchemaInquiryManager());
			}
		}

		protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion Protected Members

	}
}
