using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Falafel.Utilities
{
    public sealed class VisibilityConverter : IValueConverter
    {
        public bool Inverse
        {
            get;
            set;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(Visibility))
            {
                throw new ArgumentOutOfRangeException("targetType", "VisibilityConverter can only convert to Visibility");
            }

            Visibility visibility = Visibility.Visible;

            if (value == null)
            {
                visibility = Visibility.Collapsed;
            }
            if (value is bool)
            {
                visibility = (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is string)
            {
                visibility = String.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (Inverse)
            {
                visibility = (visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }

            return visibility;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Visibility))
            {
                throw new ArgumentOutOfRangeException("value", "VisibilityConverter can only convert from Visibility");
            }

            if (targetType == typeof(bool))
            {
                return ((Visibility)value == Visibility.Visible) ? true : false;
            }

            throw new ArgumentOutOfRangeException("targetType", "VisibilityConverter can only convert to Boolean");
        }
    }
}