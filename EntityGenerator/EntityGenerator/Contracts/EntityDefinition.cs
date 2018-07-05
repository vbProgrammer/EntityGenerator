using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EntityGenerator.Contracts
{
	public class EntityDefinition : INotifyPropertyChanged
	{
		#region EntityDefinition Members

		private string mEntityName;
		public string EntityName
		{
			get
			{
				return mEntityName;
			}
			set
			{
				mEntityName = value;
				OnPropertyChanged();
			}
		}

		public string TableName
		{
			get;
			set;
		}

		#endregion EntityDefinition Members

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
