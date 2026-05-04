using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FiveMVehicleMetaEditorWPF.Core.Converters
{
    public class BoolToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            // Also handle null check for objects (e.g. SelectedVehicle)
            return value != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
                return vis == Visibility.Collapsed;
            return true;
        }
    }
}
