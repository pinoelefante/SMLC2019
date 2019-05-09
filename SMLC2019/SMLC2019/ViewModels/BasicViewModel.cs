using Acr.UserDialogs;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SMLC2019.ViewModels
{
    public class BasicViewModel : ViewModelBase, INotifyPropertyChanged
    {

        public virtual async Task NavigatedToAsync(object o = null)
        {
            await Task.CompletedTask;
        }

        public virtual async Task NavigatedFromAsync(object o = null)
        {
            await Task.CompletedTask;
        }

        public void SetMT<T>(ref T t, T value, [CallerMemberName]string fieldName="")
        {
            t = value;
            Device.BeginInvokeOnMainThread(() =>
            {
                RaisePropertyChanged(fieldName);
            });
        }

        public async Task<bool> DisplayBasicAlert(string message, string title="", string confirm="OK", string cancel="Annulla")
        {
            return await App.Current.MainPage.DisplayAlert(title, message, confirm, cancel);
        }

        public void ShowToast(string message)
        {
            UserDialogs.Instance.Toast(message, TimeSpan.FromSeconds(3));
        }
    }
}
