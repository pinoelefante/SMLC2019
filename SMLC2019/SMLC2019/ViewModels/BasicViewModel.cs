using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SMLC2019.ViewModels
{
    public class BasicViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual async Task NavigatedToAsync(object o = null)
        {
            await Task.CompletedTask;
        }

        public virtual async Task NavigatedFromAsync(object o = null)
        {
            await Task.CompletedTask;
        }

        public void Set<T>(ref T t, T value, [CallerMemberName]string fieldName="")
        {
            t = value;
            Device.BeginInvokeOnMainThread(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(fieldName));
            });
        }
    }
}
