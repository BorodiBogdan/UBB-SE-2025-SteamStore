using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace SteamStore.Pages
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string status = value.ToString();
            string targetStatus = parameter.ToString();

            return status.Equals(targetStatus, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 