using SMLC2019.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SMLC2019.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AggiungiVoto1 : ContentPage
    {
        public AggiungiVoto1()
        {
            InitializeComponent();
            VM.PropertyChanged += VM_PropertyChanged;
        }

        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(VM.AltreSchede):
                    PopolaAltreSchede();
                    break;
            }
        }

        private AggiungiVoti1ViewModel VM => this.BindingContext as AggiungiVoti1ViewModel;
        private async void ApriPickerPartito(object sender, EventArgs e)
        {
            if (!VM.ElencoPartiti.Any())
            {
                await VM.CaricaAssets();
            }
            pickerPartito.Focus();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Factory.StartNew(async () =>
            {
                await VM.NavigatedToAsync();
            });
        }

        private void PopolaAltreSchede()
        {
            altreSchedeContainer.Children.Clear();
            foreach (var c in VM.AltreSchede)
            {
                Button b = new Button()
                {
                    Text = c.cognome
                };
                b.Clicked += (s, e) =>
                  {
                      if (c.sesso.Equals("N", StringComparison.CurrentCultureIgnoreCase) || c.sesso.Equals("M", StringComparison.CurrentCultureIgnoreCase))
                          VM.InserisciScheda(c.partito, c.id, null);
                      else if (c.sesso.Equals("F", StringComparison.CurrentCultureIgnoreCase))
                          VM.InserisciScheda(c.partito, null, c.id);
                  };
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        altreSchedeContainer.Children.Add(b);
                    }
                    catch { }
                });
            }
        }

        private void ListVoti_Refreshing(object sender, EventArgs e)
        {
            listVoti.IsRefreshing = false;
        }
    }
}