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
        public int LimiteVotiVisualizzati { get; set; } = 5;
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
            CaricaUltimiVoti();
        }

        private int seggio;
        private Partito partitoSelezionato = null;

        public int NumeroSeggio { get => seggio; set => Set(ref seggio, value); }

        public ObservableCollection<Partito> ElencoPartiti { get; } = new ObservableCollection<Partito>();
        public Partito PartitoSelezionato
        {
            get => partitoSelezionato;
            set
            {
                Set(ref partitoSelezionato, value);
                CaricaCandidati(value);
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
                    var maschi = elencoCandidati[p].Where(x => x.sesso.Equals("M", StringComparison.CurrentCultureIgnoreCase));
                    ElencoCandidatiMaschi.AddRange(maschi);

                    ElencoCandidatiFemmine.Add(emptyCandidato);
                    var femmine = elencoCandidati[p].Where(x => x.sesso.Equals("F", StringComparison.CurrentCultureIgnoreCase));
                    ElencoCandidatiFemmine.AddRange(femmine);
                }
            });
        }

        private async Task CaricaAssets()
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
            Populate(assets.partiti, assets.candidati);
            
            db.SaveItems(assets.partiti);
            db.SaveItems(assets.candidati);
        }

        private void Populate(IEnumerable<Partito> partiti, IEnumerable<Candidato> candidati)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var altreSchede = candidati.Where(x => x.sesso.Equals("N", StringComparison.CurrentCultureIgnoreCase));

                ElencoPartiti.AddRange(partiti.Where(x => x.sindaco != null).OrderBy(x => x.ordine));
                foreach (var p in partiti)
                {
                    elencoCandidati.Add(p, new List<Candidato>());
                    elencoCandidati[p].AddRange(candidati.Where(x => x.partito == p.id).OrderBy(x => x.cognome).ThenBy(x => x.nome));
                }
                IsAssetsToLoad = !elencoCandidati.Any();
            });
        }

        public ObservableCollection<VotoWrapped> UltimiVoti { get; } = new ObservableCollection<VotoWrapped>();

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

                AggiungiUltimoVoto(v);

                Device.BeginInvokeOnMainThread(() =>
                {
                    FemminaSelezionata = null;
                    MaschioSelezionato = null;
                    PartitoSelezionato = null;
                });
            }));

        private void AggiungiUltimoVoto(Voto v)
        {
            var p = ElencoPartiti.FirstOrDefault(x => x.id == v.partito);

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
    }

    public static class ObservableCollectionExtension
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            if (items == null)
                return;
            foreach(var item in items)
                collection.Add(item);
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
