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
	public partial class AggiungiVoto1 : ContentPage
	{
		public AggiungiVoto1 ()
		{
			InitializeComponent ();

            BindingContext = new AggiungiVoti1ViewModel();
		}

        private void ApriPickerPartito(object sender, EventArgs e)
        {
            pickerPartito.Focus();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Factory.StartNew(async () =>
            {
                await (BindingContext as AggiungiVoti1ViewModel).NavigatedToAsync();
            });
        }
    }
}