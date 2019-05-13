using GalaSoft.MvvmLight.Command;
using Rg.Plugins.Popup.Services;
using SMLC2019.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMLC2019.ViewModels
{
    public class SettingsViewModel : BasicViewModel
    {
        public Configuration Config { get; private set; }
        private readonly ServerAPI api;

        private RelayCommand chiudiCommand;
        public RelayCommand ChiudiCommand => chiudiCommand ?? (chiudiCommand = new RelayCommand(ChiudiImpostazioniAsync));
        public SettingsViewModel(Configuration conf, IToast t, ServerAPI s) : base(t)
        {
            Config = conf;
            api = s;
        }

        private async void ChiudiImpostazioniAsync()
        {
            Config.InizializzaAPI();
            Config.SalvaConfigurazione();
            await PopupNavigation.Instance.PopAsync(true);
        }
    }
}
