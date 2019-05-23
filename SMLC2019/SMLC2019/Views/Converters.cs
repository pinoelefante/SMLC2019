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
            return $"{candidato.cognome} {(!string.IsNullOrEmpty(candidato.nome) ? candidato.nome.First()+"." : string.Empty)}".Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ListName2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var candidato = value as Candidato;
            if (candidato == null)
                return string.Empty;
            return $"{candidato.cognome} {(candidato.nome ?? string.Empty)}".Trim();
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

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotIsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            int val = (int)value;
            if (int.TryParse(parameter?.ToString(), out int than))
                return val > than;
            return val > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CandidatoBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var candidato = value as Candidato;
            if (candidato == null)
                return null;
            switch(candidato.sesso.ToUpper())
            {
                case "M":
                    return Color.Aqua;
                case "F":
                    return Color.LightPink;
            }
            return Color.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CandidatoImageFromCollection : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<Candidato> candidati = value as IEnumerable<Candidato>;
            if (value == null)
                return null;
            try
            {
                int index = System.Convert.ToInt32(parameter);
                if (index >= candidati.Count())
                    return null;
                return candidati.ElementAt(index).foto;
            }
            catch
            {

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            var v = System.Convert.ToInt32(value);
            var p = System.Convert.ToInt32(parameter);
            return v == p;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
