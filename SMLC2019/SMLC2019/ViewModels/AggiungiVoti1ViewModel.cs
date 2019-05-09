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
        private ServerAPI api;
        private DatabaseService db;

        protected Dictionary<Partito, List<Candidato>> elencoCandidati;
        private int votiCaricare, seggio;
        private Partito partitoSelezionato = null;
        private Candidato maschioSelezionato, femminaSelezionata, emptyCandidato = new Candidato() { cognome = "(Nessun voto)" };
        private List<Candidato> altreSchede;
        private ICommand aggiungiCommand;

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
        public ObservableCollection<Candidato> ElencoCandidatiMaschi { get; } = new ObservableCollection<Candidato>();
        public ObservableCollection<Candidato> ElencoCandidatiFemmine { get; } = new ObservableCollection<Candidato>();
        public Candidato MaschioSelezionato { get => maschioSelezionato; set => Set(ref maschioSelezionato, value == emptyCandidato ? null : value); }
        public Candidato FemminaSelezionata { get => femminaSelezionata; set => Set(ref femminaSelezionata, value == emptyCandidato ? null : value); }
        public List<Candidato> AltreSchede { get => altreSchede; set => Set(ref altreSchede, value); }
        public ObservableCollection<VotoWrapped> UltimiVoti { get; } = new ObservableCollection<VotoWrapped>();
        public int VotiCaricare { get => votiCaricare; set => Set(ref votiCaricare, value); }
        public int LimiteVotiVisualizzati { get; set; } = 20;
        public ICommand AggiungiCommand =>
            aggiungiCommand ??
            (aggiungiCommand = new Command(() =>
            {
                if (PartitoSelezionato == null)
                    return;

                InserisciScheda(PartitoSelezionato.id, MaschioSelezionato?.id, FemminaSelezionata?.id);
            }));

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
            Populate(assets.partiti, assets.candidati);
            
            db.SaveItems(assets.partiti);
            db.SaveItems(assets.candidati);
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
            }
            else
            {
                // Mostrare errore
            }
        }

        private void AggiungiUltimoVoto(Voto v)
        {
            var p = elencoCandidati.Keys.FirstOrDefault(x => x.id == v.partito);

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
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items, bool clear=false)
        {
            if (items == null)
                return;
            if (clear)
                collection.Clear();
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
