using pinoelefante.Services;
using SMLC2019.Models;
using SMLC2019.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SMLC2019.ViewModels
{
    public class AggiungiVoti1ViewModel : BasicViewModel
    {
        public Dictionary<Partito, List<Candidato>> elencoCandidati;
        private ServerAPI api;
        private DatabaseService db;
        public AggiungiVoti1ViewModel()
        {
            elencoCandidati = new Dictionary<Partito, List<Candidato>>();
            api = new ServerAPI();
            api.Endpoint = "https://pinoelefante.altervista.org/smlc19/endpoint.php";
            var sqlite = DependencyService.Get<ISQLite>();
            db = new DatabaseService(sqlite);
        }

        public override async Task NavigatedToAsync(object o = null)
        {
            if (!elencoCandidati.Any())
                await CaricaAssets();
            
        }

        private int seggio;
        private Partito partitoSelezionato;

        public int NumeroSeggio { get => seggio; set => Set(ref seggio, value); }

        public ObservableCollection<Partito> ElencoPartiti { get; } = new ObservableCollection<Partito>();
        public Partito PartitoSelezionato
        {
            get => partitoSelezionato;
            set
            {
                Set(ref partitoSelezionato, value);
                CaricaCandidati(partitoSelezionato);
            }
        }

        private bool isAssetsToLoad = true;
        public bool IsAssetsToLoad { get => isAssetsToLoad; set => Set(ref isAssetsToLoad, value); }

        private Candidato maschioSelezionato, femminaSelezionata;

        public Candidato MaschioSelezionato
        {
            get => maschioSelezionato;
            set => Set(ref maschioSelezionato, value == emptyCandidato ? null : value);
        }

        public Candidato FemminaSelezionata
        {
            get => femminaSelezionata;
            set => Set(ref femminaSelezionata, value == emptyCandidato ? null : value);
        }

        public ObservableCollection<Candidato> ElencoCandidatiMaschi { get; } = new ObservableCollection<Candidato>();
        public ObservableCollection<Candidato> ElencoCandidatiFemmine { get; } = new ObservableCollection<Candidato>();

        private Candidato emptyCandidato = new Candidato() { cognome="(Nessun voto)" };
        private void CaricaCandidati(Partito p)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ElencoCandidatiMaschi.Clear();
                ElencoCandidatiFemmine.Clear();
                if (p != null)
                {
                    ElencoCandidatiMaschi.Add(emptyCandidato);
                    ElencoCandidatiMaschi.AddRange(elencoCandidati[p].Where(x => x.sesso.Equals("M", StringComparison.CurrentCultureIgnoreCase)));
                    ElencoCandidatiFemmine.Add(emptyCandidato);
                    ElencoCandidatiFemmine.AddRange(elencoCandidati[p].Where(x => x.sesso.Equals("F", StringComparison.CurrentCultureIgnoreCase)));
                }
            });
        }

        private async Task CaricaAssets()
        {
            var partiti = db.GetAllPartiti().OrderBy(x => x.ordine);
            var candidati = db.GetAllCandidati();
            if (partiti == null || !partiti.Any() || candidati == null || !candidati.Any())
            {
                
            }
        }

        private async Task CaricaAssetsOnlineAsync()
        {
            var assets = await api.GetAssetsAsync();
            if (assets == null)
                return;
            Device.BeginInvokeOnMainThread(() =>
            {
                ElencoPartiti.AddRange(assets.partiti.Where(x => x.sindaco != null).OrderBy(x => x.ordine));
                foreach (var p in assets.partiti)
                {
                    elencoCandidati.Add(p, new List<Candidato>());
                    elencoCandidati[p].AddRange(assets.candidati.Where(x => x.partito == p.id).OrderBy(x => x.cognome).ThenBy(x => x.nome));
                }
                IsAssetsToLoad = !elencoCandidati.Any();
            });
        }

        private ICommand aggiungiCommand;
        public ICommand AggiungiCommand =>
            aggiungiCommand ??
            (aggiungiCommand = new Command(() =>
            {
                if (PartitoSelezionato == null)
                    return;
                Voto v = new Voto()
                {
                    seggio = NumeroSeggio,
                    partito = PartitoSelezionato.id,
                    tempo = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
                    femmina = FemminaSelezionata?.id,
                    maschio = MaschioSelezionato?.id
                };
                db.SaveItem(v);
                
                FemminaSelezionata = null;
                MaschioSelezionato = null;
                PartitoSelezionato = null;
            }));

    }

    public static class ObservableCollectionExtension
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
