using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EntityGenerator.UI.Converters
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		#region BooleanToVisibilityConverter Members

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			Visibility result = Visibility.Collapsed;
			if( value is bool )
			{
				var typedValue = (bool)value;
				result = typedValue
					? Visibility.Visible
					: Visibility.Collapsed;
			}
			return result;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}

		#endregion BooleanToVisibilityConverter Members

	}
}
