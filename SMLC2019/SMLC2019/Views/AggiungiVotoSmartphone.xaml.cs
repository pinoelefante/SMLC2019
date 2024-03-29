﻿using SMLC2019.ViewModels;
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
    public partial class AggiungiVotoSmartphone : ContentPage
    {
        public AggiungiVotoSmartphone()
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
                case nameof(VM.IsInviandoVoti):
                    break;
            }
        }

        private AggiungiVotiSmartphone VM => this.BindingContext as AggiungiVotiSmartphone;
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
                          VM.InserisciAltraScheda(c.partito, c.id, null, c.cognome);
                      else if (c.sesso.Equals("F", StringComparison.CurrentCultureIgnoreCase))
                          VM.InserisciAltraScheda(c.partito, null, c.id, c.cognome);
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

    }
}