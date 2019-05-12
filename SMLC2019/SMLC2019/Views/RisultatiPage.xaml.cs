using SMLC2019.ViewModels;
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
    public partial class RisultatiPage : ContentPage
    {
        public RisultatiPage()
        {
            InitializeComponent();
        }
        private RisultatiViewModel VM => this.BindingContext as RisultatiViewModel;

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await VM.NavigatedToAsync();
        }
    }
}