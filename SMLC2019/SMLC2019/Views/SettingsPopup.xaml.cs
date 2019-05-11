using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
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
    public partial class SettingsPopup : PopupPage
    {
        public SettingsPopup()
        {
            InitializeComponent();
        }
        private SettingsViewModel VM => this.BindingContext as SettingsViewModel;
        private async void ClosePopup(object sender, EventArgs e)
        {
            VM.Config.SalvaConfigurazione();
            await PopupNavigation.Instance.PopAsync(true);
        }
    }
}