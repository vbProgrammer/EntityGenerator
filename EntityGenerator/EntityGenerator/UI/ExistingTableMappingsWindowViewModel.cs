using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Practices.Prism.Commands;

namespace EntityGenerator.UI
{
	public class ExistingTableMappingsWindowViewModel
	{
		#region Constructors

		public ExistingTableMappingsWindowViewModel( ExistingTableMappingsWindowPayload payload )
		{
			MatchedFiles = payload.MatchedFiles;
			TableName = payload.TableName;
		}

		#endregion Constructors

		#region ExistingTableMappingsWindowViewModel Members

		private DelegateCommand mCancelCommand;
		public DelegateCommand CancelCommand
		{
			get
			{
				return mCancelCommand ?? (mCancelCommand = new DelegateCommand( _HandleCancelCommandExecuted ));
			}
		}

		public List<FileInfo> MatchedFiles
		{
			get;
			set;
		}

		public bool OkClicked
		{
			get;
			set;
		}

		private DelegateCommand mOkCommand;
		public DelegateCommand OkCommand
		{
			get
			{
				return mOkCommand ?? (mOkCommand = new DelegateCommand( _HandleOkCommandCommandExecuted ));
			}
		}

		public string TableName
		{
			get;
			set;
		}

		public void OnFinished( bool okClicked )
		{
			var handler = Finished;
			OkClicked = okClicked;
			if( handler != null )
			{
				handler( this, EventArgs.Empty );
			}
		}

		#endregion ExistingTableMappingsWindowViewModel Members

		#region Ëvents

		public event EventHandler Finished;

		#endregion Ëvents

		#region Private Members

		private void _HandleCancelCommandExecuted()
		{
			OnFinished( false );
		}

		private void _HandleOkCommandCommandExecuted()
		{
			OnFinished( true );
		}

		#endregion Private Members

	}
}
