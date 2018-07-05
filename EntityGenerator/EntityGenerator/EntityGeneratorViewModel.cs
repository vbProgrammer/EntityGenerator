using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

using EntityGenerator.UI;

namespace EntityGenerator
{
	public class EntityGeneratorViewModel : INotifyPropertyChanged
	{
		#region Constructors

		public static event EventHandler ExitingRequest;

		public EntityGeneratorViewModel()
		{
			CurrentViewModel = this;

			var tableSelectionView = new TableSelectionView();
			NavigateTo( tableSelectionView );
		}

		public EntityGeneratorViewModel( string filePath )
		{
			CurrentViewModel = this;

			var tableSelectionView = new TableSelectionView( filePath );
			NavigateTo( tableSelectionView );
		}

		static EntityGeneratorViewModel()
		{
			NavigationStack = new Stack<ContentControl>();
		}

		#endregion Constructors

		#region EntityGeneratorViewModel Members

		private string mBusyText;
		public string BusyText
		{
			get
			{
				return mBusyText;
			}
			set
			{
				mBusyText = value;
				OnPropertyChanged();
			}
		}

		private ContentControl mCurrentView;
		public ContentControl CurrentView
		{
			get
			{
				return mCurrentView;
			}
			set
			{
				mCurrentView = value;
				OnPropertyChanged();
			}
		}

		public static EntityGeneratorViewModel CurrentViewModel
		{
			get;
			set;
		}

		private bool mIsBusy;
		public bool IsBusy
		{
			get
			{
				return mIsBusy;
			}
			set
			{
				mIsBusy = value;
				OnPropertyChanged();
			}
		}

		public static void HideBusy()
		{
			CurrentViewModel.IsBusy = false;
		}

		public static void NavigateTo( ContentControl newView )
		{
			if( CurrentViewModel.CurrentView != null )
			{
				NavigationStack.Push( CurrentViewModel.CurrentView );
			}
			CurrentViewModel.CurrentView = newView;
		}

		public static void PopViewModel()
		{
			if( NavigationStack.Any() )
			{
				CurrentViewModel.CurrentView = NavigationStack.Pop();
			}
		}

		public static void Exit()
		{
			ExitingRequest(CurrentViewModel.CurrentView, EventArgs.Empty);
		}

		public static bool CanExit()
		{
			return ExitingRequest != null;
		}

		public static void ShowBusy( string busyText )
		{
			CurrentViewModel.BusyText = busyText ?? "Busy...";
			CurrentViewModel.IsBusy = true;
		}

		#endregion EntityGeneratorViewModel Members

		#region Ëvents

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Ëvents

		#region Protected Members

		protected static Stack<ContentControl> NavigationStack
		{
			get;
			set;
		}

		protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion Protected Members

	}
}
