using GalaSoft.MvvmLight.Command;
using pinoelefante.Services;
using Rg.Plugins.Popup.Services;
using SMLC2019.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SMLC2019.Models;
using Xamarin.Essentials;

namespace SMLC2019.ViewModels
{
    public class SettingsViewModel : BasicViewModel
    {
        public Configuration Config { get; private set; }
        private readonly ServerAPI api;
        private readonly DatabaseService db;

        private RelayCommand chiudiCommand, cancellaTutto, verificaDati, forzaInvio;
        public RelayCommand ChiudiCommand => chiudiCommand ?? (chiudiCommand = new RelayCommand(ChiudiImpostazioniAsync));
        public RelayCommand CancellaDBCommand => cancellaTutto ?? (cancellaTutto = new RelayCommand(CancellaDBAsync));
        public RelayCommand VerificaDatiCommand => verificaDati ?? (verificaDati = new RelayCommand(VerificaConnessioneAsync));
        public RelayCommand ForzaInvioCommand => forzaInvio ?? (forzaInvio = new RelayCommand(ForzaInvio));
        public SettingsViewModel(Configuration conf, IToast t, ServerAPI s, DatabaseService d) : base(t)
        {
            Config = conf;
            api = s;
            db = d;
        }

        private async void ChiudiImpostazioniAsync()
        {
            if(Config.Seggio < 1)
            {
                ShowToast("Selezionare un seggio");
                return;
            }
            if(string.IsNullOrEmpty(Config.Endpoint) || !Uri.IsWellFormedUriString(Config.Endpoint, UriKind.Absolute))
            {
                ShowToast("Inserire un endpoint");
                return;
            }
            Config.InizializzaAPI();
            Config.SalvaConfigurazione();
            await PopupNavigation.Instance.PopAsync(true);
            MessengerInstance.Send<bool>(true, "ImpostazioniChiuse");
        }

        private async void CancellaDBAsync()
        {
            if (await DisplayBasicAlert("Sei sicuro/a di voler cancellare tutti i voti inseriti?\nL'operazione è irreversibile.\n", "Cancellazione dati"))
                db.TruncateTable<Voto>();

            Config.CancellaUltimoInvio(1, 12);
            Config.CancellaVotiDaEliminare();

            MessengerInstance.Send(true, "RicaricaVoti");
        }

        private async void VerificaConnessioneAsync()
        {
            api.Endpoint = Config.Endpoint;
            if (!string.IsNullOrEmpty(Config.Username) && !string.IsNullOrEmpty(Config.Password))
                api.SetAuthentication(Config.Username, Config.Password);
            var res = await api.Ping();
            ShowToast($"Dati inseriti: {(res ? "Validi" : "Invalidi")}");
        }

        private async void ForzaInvio()
        {
            if(Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                ShowToast("Connessione assente");
                return;
            }
            var seggio = Config.Seggio;
            if (!await DisplayBasicAlert($"Seggio {seggio}\nUtilizzando questa funzione verranno prima cancellati i voti presenti online e poi caricati tutti i voti presenti sul dispositivo.\nAssicurati di trovarti in una zona con una buona copertura internet", "Invio forzato", "Si", "No"))
                return;
            if(await api.CancellaVoti(seggio))
            {
                Config.CancellaVotiDaEliminare();
                var voti = db.GetVotiDaCaricare(seggio, 0);
                if (voti.Any())
                {
                    var res = await api.AggiungiVotiAsync(voti);
                    ShowToast($"Invio forzato: {(res ? "OK" : "ERRORE")}");
                    if (res)
                    {
                        var max = voti.Max(x => x.tempo);
                        Config.SalvaUltimoInvio(seggio, max);
                        MessengerInstance.Send(true, "RicaricaVoti");
                    }
                }
                else
                {
                    ShowToast("Non ci sono voti da caricare");
                }
            }
            else
            {
                ShowToast("Errore");
            }
        }
    }
}
