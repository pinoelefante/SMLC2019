﻿using SMLC2019.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SMLC2019.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AggiungiVoto2 : ContentPage
    {
        public AggiungiVoto2()
        {
            InitializeComponent();
        }
        private AggiungiVoti2ViewModel VM => this.BindingContext as AggiungiVoti2ViewModel;

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await VM.NavigatedToAsync();
        }
    }
}