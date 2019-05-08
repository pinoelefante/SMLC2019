using SMLC2019.Models;
using SMLC2019.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Xamarin.Forms;
using System.Linq;

namespace SMLC2019.Views
{
    public class StringImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var image = ImageSource.FromResource($"SMLC2019.Images.{value}");
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = (value as StreamImageSource);
            Console.WriteLine("ConvertBack called");
            return value;
        }
    }
    public class ListNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var candidato = value as Candidato;
            if (candidato == null)
                return string.Empty;
            return $"{candidato.cognome} {(candidato!=null ? candidato.nome.First()+"." : string.Empty)}".Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UnixTimestampTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var seconds = System.Convert.ToInt32(value);
                var date = new DateTime(1970, 1, 1).AddSeconds(seconds);
                return $"{date.Hour.ToString("D2")}:{date.Minute.ToString("D2")}.{date.Second.ToString("D2")}";
            }
            catch { }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
