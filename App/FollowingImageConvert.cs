using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace App
{
    public class FollowingImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFollowing)
            {
                return isFollowing ? "minus.svg" : "add.svg";
            }
            return "add.svg";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
