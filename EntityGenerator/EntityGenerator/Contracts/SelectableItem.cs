using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EntityGenerator.Contracts
{
	public class SelectableItem : INotifyPropertyChanged
	{
		#region SelectableItem Members

		public string Description
		{
			get;
			set;
		}

		private bool mIsSelected;
		public bool IsSelected
		{
			get
			{
				return mIsSelected;
			}
			set
			{
				mIsSelected = value;
				OnPropertyChanged();
			}
		}

		#endregion SelectableItem Members

		#region Ëvents

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Ëvents

		#region Protected Members

		protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion Protected Members

	}
}
