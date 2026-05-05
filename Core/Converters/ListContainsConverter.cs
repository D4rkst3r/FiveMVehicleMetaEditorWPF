using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace FiveMVehicleMetaEditorWPF.Core.Converters
{
    /// <summary>
    /// Converts List&lt;string&gt; + ConverterParameter (flag name) → bool IsChecked.
    /// ConvertBack adds/removes the flag from the list.
    /// </summary>
    public class ListContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string> list && parameter is string flag)
                return list.Contains(flag);
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is not useful here since we can't return the mutated list.
            // The CheckBox.Checked/Unchecked events handle it via code-behind or commands.
            return Binding.DoNothing;
        }
    }
}
