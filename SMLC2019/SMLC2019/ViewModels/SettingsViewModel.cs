using GalaSoft.MvvmLight.Command;
using pinoelefante.Services;
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
        private readonly DatabaseService db;

        private RelayCommand chiudiCommand, cancellaTutto, verificaDati;
        public RelayCommand ChiudiCommand => chiudiCommand ?? (chiudiCommand = new RelayCommand(ChiudiImpostazioniAsync));
        public RelayCommand CancellaDBCommand => cancellaTutto ?? (cancellaTutto = new RelayCommand(CancellaDBAsync));
        public RelayCommand VerificaDatiCommand => verificaDati ?? (verificaDati = new RelayCommand(VerificaConnessioneAsync));
        public SettingsViewModel(Configuration conf, IToast t, ServerAPI s, DatabaseService d) : base(t)
        {
            Config = conf;
            api = s;
            db = d;
        }

        private async void ChiudiImpostazioniAsync()
        {
            Config.InizializzaAPI();
            Config.SalvaConfigurazione();
            await PopupNavigation.Instance.PopAsync(true);
        }

        private async void CancellaDBAsync()
        {
            if(await DisplayBasicAlert("Sei sicuro/a di voler cancellare tutti i dati inseriti?\nL'operazione è irreversibile.", "Cancellazione dati"))
                db.DeleteDatabase();
        }

        private async void VerificaConnessioneAsync()
        {
            var res = await api.Ping();
            ShowToast($"Dati inseriti: {(res ? "Validi" : "Invalidi")}");
        }
    }
}
