using GalaSoft.MvvmLight.Command;
using pinoelefante.Services;
using Rg.Plugins.Popup.Services;
using SMLC2019.Models;
using SMLC2019.Services;
using SMLC2019.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using SMLC2019.Extensions;
using System.Threading;

namespace SMLC2019.ViewModels
{
    public class AggiungiVotiSmartphone : BasicViewModel
    {
        private ServerAPI api;
        private DatabaseService db;
        private Configuration conf;

        protected Dictionary<Partito, List<Candidato>> elencoCandidati;
        private int votiCaricare, seggio;
        private long tempoUltimoInvio;
        private Partito partitoSelezionato = null;
        private Candidato maschioSelezionato, femminaSelezionata, emptyCandidato = new Candidato() { cognome = "(Nessun voto)" };
        private List<Candidato> altreSchede;
        private ICommand aggiungiCommand, cancellaVoto, inviaVoti, aggiornaVoti, apriImpostazioni;
        private VotoWrapped votoSelezionato;
        private bool isInviandoVoti = false;
        private Timer invioTimer;

        public int NumeroSeggio { get => seggio; set => SetMT(ref seggio, value); }
        public ObservableCollection<Partito> ElencoPartiti { get; } = new ObservableCollection<Partito>();
        public Partito PartitoSelezionato
        {
            get => partitoSelezionato;
            set
            {
                SetMT(ref partitoSelezionato, value);
                MaschioSelezionato = null;
                FemminaSelezionata = null;
                CaricaCandidati(value);
            }
        }
        public ObservableCollection<Candidato> ElencoCandidatiMaschi { get; } = new ObservableCollection<Candidato>();
        public ObservableCollection<Candidato> ElencoCandidatiFemmine { get; } = new ObservableCollection<Candidato>();
        public Candidato MaschioSelezionato { get => maschioSelezionato; set => SetMT(ref maschioSelezionato, value == emptyCandidato ? null : value); }
        public Candidato FemminaSelezionata { get => femminaSelezionata; set => SetMT(ref femminaSelezionata, value == emptyCandidato ? null : value); }
        public List<Candidato> AltreSchede { get => altreSchede; set => SetMT(ref altreSchede, value); }
        public ObservableCollection<VotoWrapped> UltimiVoti { get; } = new ObservableCollection<VotoWrapped>();
        public int VotiCaricare { get => votiCaricare; set => SetMT(ref votiCaricare, value); }
        public int LimiteVotiVisualizzati { get; set; } = 20;
        public VotoWrapped VotoSelezionato { get => votoSelezionato; set => SetMT(ref votoSelezionato, value); }
        public bool IsInviandoVoti { get => isInviandoVoti; set => SetMT(ref isInviandoVoti, value); }
        public ICommand AggiungiCommand =>
            aggiungiCommand ??
            (aggiungiCommand = new RelayCommand(() =>
            {
                if (PartitoSelezionato == null)
                    return;
                InserisciScheda(PartitoSelezionato.id, MaschioSelezionato?.id, FemminaSelezionata?.id);
            }));
        public ICommand CancellaVotoCommand =>
            cancellaVoto ??
            (cancellaVoto = new RelayCommand(async () =>
            {
                if (VotoSelezionato == null)
                    return;
                bool remoteDelete = VotoSelezionato.Voto.tempo <= tempoUltimoInvio;
                if (!await DisplayBasicAlert("Vuoi eliminare il voto?\n"+(remoteDelete ? "Il voto sarà anche eliminato dai voti già inviati": ""), "Conferma eliminazione", "Si", "No"))
                    return;
                if (db.Delete(VotoSelezionato.Voto))
                {
                    if(remoteDelete)
                    {
                        var res = await api.CancellaVoto(VotoSelezionato.Voto.seggio, VotoSelezionato.Voto.tempo);
                        if(!res)
                        {
                            conf.AggiungiVotoDaEliminare(VotoSelezionato.Voto.seggio, VotoSelezionato.Voto.tempo);
                        }
                    }
                    else
                        VotiCaricare--;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        UltimiVoti.Remove(VotoSelezionato);
                        VotoSelezionato = null;
                        if(!UltimiVoti.Any())
                            CaricaUltimiVoti();
                    });
                }
                else
                    ShowToast("Voto non eliminato");
            }));
        public ICommand AggiornaVotiCommand =>
            aggiornaVoti ??
            (aggiornaVoti = new RelayCommand(CaricaUltimiVoti));

        public ICommand ApriImpostazioniCommand => apriImpostazioni ?? (apriImpostazioni = new RelayCommand(() => ApriImpostazioni()));
        public ICommand InviaVotiCommand => inviaVoti ?? (inviaVoti = new RelayCommand(() => InviaVoti()));
        public AggiungiVotiSmartphone(ServerAPI s, DatabaseService d, Configuration c, IToast t) : base(t)
        {
            elencoCandidati = new Dictionary<Partito, List<Candidato>>();
            api = s;
            db = d;
            conf = c;
            c.PropertyChanged += Configuration_PropertyChanged;
            
            Inizializza();

            invioTimer = new Timer(InviaVotiBG, null, 5000, 60000);
            
        }

        private void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(conf.Seggio != NumeroSeggio)
            {
                NumeroSeggio = conf.Seggio;
                CaricaUltimiVoti();
                tempoUltimoInvio = conf.UltimoInvio;
            }
        }

        public override async Task NavigatedToAsync(object o = null)
        {
            if (NumeroSeggio <= 0)
                ApriImpostazioni();
            await Task.CompletedTask;
        }

        private async void Inizializza()
        {
            if (!elencoCandidati.Any())
                await CaricaAssets();
            NumeroSeggio = conf.Seggio;
            tempoUltimoInvio = conf.UltimoInvio;
            VotiCaricare = db.GetVotiDaCaricareCount(NumeroSeggio, tempoUltimoInvio);
            CaricaUltimiVoti();
        }

        protected virtual void CaricaCandidati(Partito p)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ElencoCandidatiMaschi.Clear();
                ElencoCandidatiFemmine.Clear();
                if (p != null)
                {
                    ElencoCandidatiMaschi.Add(emptyCandidato);
                    var maschi = elencoCandidati[p].Where(x => x.sesso.Equals("M", StringComparison.CurrentCultureIgnoreCase));
                    ElencoCandidatiMaschi.AddRange(maschi);

                    ElencoCandidatiFemmine.Add(emptyCandidato);
                    var femmine = elencoCandidati[p].Where(x => x.sesso.Equals("F", StringComparison.CurrentCultureIgnoreCase));
                    ElencoCandidatiFemmine.AddRange(femmine);
                }
            });
        }

        public async Task CaricaAssets()
        {
            var partiti = db.GetAllPartiti().OrderBy(x => x.ordine);
            var candidati = db.GetAllCandidati();
            if (partiti == null || !partiti.Any() || candidati == null || !candidati.Any())
            {
                Debug.WriteLine("Carico assets dal web");
                await CaricaAssetsOnlineAsync();
            }
            else
            {
                Debug.WriteLine("Carico assets dal db");
                Populate(partiti, candidati);
            }
        }

        private async Task CaricaAssetsOnlineAsync()
        {
            var assets = await api.GetAssetsAsync();
            if (assets == null)
                return;
            Populate(assets.Partiti, assets.Candidati);
            
            db.SaveItems(assets.Partiti);
            db.SaveItems(assets.Candidati);
        }
        
        private void Populate(IEnumerable<Partito> partiti, IEnumerable<Candidato> candidati)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var altreSchede = candidati.Where(x => x.sesso.Equals("N", StringComparison.CurrentCultureIgnoreCase)).ToList();
                AltreSchede = altreSchede;
                
                ElencoPartiti.AddRange(partiti.Where(x => x.sindaco != null).OrderBy(x => x.ordine), true);
                foreach (var p in partiti)
                {
                    elencoCandidati.Add(p, new List<Candidato>());
                    elencoCandidati[p].AddRange(candidati.Where(x => x.partito == p.id).OrderBy(x => x.cognome).ThenBy(x => x.nome));
                }
            });
        }

        private void CaricaUltimiVoti()
        {
            var voti = db.GetLastVoti(NumeroSeggio, LimiteVotiVisualizzati);
            Device.BeginInvokeOnMainThread(() =>
            {
                UltimiVoti.Clear();
                foreach (var v in voti.OrderBy(x => x.tempo))
                    AggiungiUltimoVoto(v);
            });
        }
        public async void InserisciAltraScheda(int partito, int? maschio, int? femmina, string nome="")
        {
            if (!await DisplayBasicAlert($"Sei sicuro/a di voler inserire la scheda{(string.IsNullOrEmpty(nome) ? string.Empty : $" {nome}")}?", "Conferma inserimento", "Si", "No"))
                return;
            InserisciScheda(partito, maschio, femmina);
        }
        public void InserisciScheda(int partito, int? maschio, int? femmina)
        {
            Voto v = new Voto()
            {
                partito = partito,
                maschio = maschio,
                femmina = femmina,
                seggio = NumeroSeggio,
                tempo = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
            };

            if (db.SaveItem(v))
            {
                AggiungiUltimoVoto(v);

                Device.BeginInvokeOnMainThread(() =>
                {
                    FemminaSelezionata = null;
                    MaschioSelezionato = null;
                    PartitoSelezionato = null;

                    VotiCaricare++;
                });
                ShowToast("Aggiunto");
            }
            else
            {
                ShowToast("Il voto non è stato inserito");
            }
        }

        private void AggiungiUltimoVoto(Voto v)
        {
            var p = elencoCandidati.Keys.FirstOrDefault(x => x.id == v.partito);
            if(p == null)
            {
                Console.WriteLine("Partito non trovato: " + v.partito);
            }
            VotoWrapped vw = new VotoWrapped(v,
                p,
                elencoCandidati[p].FirstOrDefault(x => x.id == v.maschio),
                elencoCandidati[p].FirstOrDefault(x => x.id == v.femmina));

            Device.BeginInvokeOnMainThread(() =>
            {
                if (UltimiVoti.Count == LimiteVotiVisualizzati)
                    UltimiVoti.RemoveAt(LimiteVotiVisualizzati-1);
                UltimiVoti.Insert(0, vw);
            });
        }

        private void InviaVotiBG(object state)
        {
            lock(this)
            {
                Console.WriteLine("Backgroung sender: Invio voti");
                InviaVoti(false);
            }
        }

        public async void InviaVoti(bool showErrors = true)
        {
            if (IsInviandoVoti)
            {
                if (showErrors)
                    ShowToast("Operazione già in corso");
                return;
            }
            if(Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                if(showErrors)
                    ShowToast("Connessione non disponibile");
                return;
            }
            IsInviandoVoti = true;
            int seggio = NumeroSeggio;

            /* Voti da eliminare */
            if(conf.DaEliminare.ContainsKey(seggio) && conf.DaEliminare[seggio].Any())
            {
                foreach(var t in conf.DaEliminare[seggio].ToList())
                {
                    if (await api.CancellaVoto(seggio, t))
                        conf.RimuoviVotoDaEliminare(seggio, t);
                }
            }

            /* Voti da caricare */
            var voti = db.GetVotiDaCaricare(seggio, tempoUltimoInvio);
            if (voti.Any())
            {
                var res = await api.AggiungiVotiAsync(voti);
                if (res)
                {
                    if(showErrors)
                        ShowToast("Voti caricati");
                    tempoUltimoInvio = voti.Max(x => x.tempo);
                    if (!conf.SalvaUltimoInvio(seggio, tempoUltimoInvio) && showErrors)
                        ShowToast("Tempo non salvato. Non chiudere l'applicazione");
                }
                else
                {
                    if (showErrors)
                        ShowToast("ERRORE: voti non caricati");
                }
            }
            VotiCaricare = db.GetVotiDaCaricareCount(seggio, tempoUltimoInvio);
            IsInviandoVoti = false;
        }
        public async void ApriImpostazioni()
        {
            await PopupNavigation.Instance.PushAsync(new SettingsPopup(), true);
        }
    }

    public class VotoWrapped
    {
        public Voto Voto { get; }
        public Partito Partito { get; }
        public Candidato Maschio { get; }
        public Candidato Femmina { get; }

        public VotoWrapped(Voto voto, Partito p, Candidato m, Candidato f)
        {
            Voto = voto;
            Partito = p;
            Maschio = m;
            Femmina = f;
        }
    }
}
