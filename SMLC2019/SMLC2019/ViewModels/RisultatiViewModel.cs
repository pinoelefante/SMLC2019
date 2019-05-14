using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using SMLC2019.Services;
using SMLC2019.Extensions;
using SMLC2019.Models;
using System.Collections.ObjectModel;
using System.Linq;
using pinoelefante.Services;
using Xamarin.Forms;

namespace SMLC2019.ViewModels
{
    public class RisultatiViewModel : BasicViewModel
    {
        private readonly ServerAPI api;
        private readonly DatabaseService db;
        private RelayCommand caricaVotiCmd;
        private int seggioSelezionato = 0;

        public int SeggioSelezionato
        {
            get => seggioSelezionato;
            set
            {
                SetMT(ref seggioSelezionato, value);
                CaricaVoti();
            }
        }
        public RelayCommand CaricaVotiCommand => caricaVotiCmd ?? (caricaVotiCmd = new RelayCommand(CaricaVoti));
        public ObservableCollection<RisultatoPartitoWrapped> RisultatiListe { get; } = new ObservableCollection<RisultatoPartitoWrapped>();
        public ObservableCollection<RisultatoCandidatoWrapped> RisultatiCandidati { get; } = new ObservableCollection<RisultatoCandidatoWrapped>();
        public RisultatiElettorali Risultati { get; set; }
        public RisultatiViewModel(IToast toast, ServerAPI a, DatabaseService d) : base(toast)
        {
            api = a;
            db = d;
        }

        public override Task NavigatedToAsync(object o = null)
        {
            CaricaVoti();
            return base.NavigatedToAsync(o);
        }

        private async void CaricaVoti()
        {
            var res = await api.GetVotiPerSeggioAsync();
            if (res == null)
                ShowToast("Si è verificato un errore durante il caricamento dei voti");
            else
            {
                Risultati = res;
                if (SeggioSelezionato < 1)
                {
                    GeneraVotiListeTotale(res);
                    GeneraVotiCandidatiTotale(res);
                }
                else
                {
                    GeneraVotiListeSeggio(res, SeggioSelezionato);
                    GeneraVotiCandidatiSeggio(res, SeggioSelezionato);
                }
            }
        }

        private void GeneraVotiListeTotale(RisultatiElettorali voti)
        {
            Dictionary<int, int> listeCounts = new Dictionary<int, int>();
            foreach(var l in voti.liste)
            {
                if (listeCounts.ContainsKey(l.id))
                {
                    var oldVoti = listeCounts[l.id];
                    listeCounts.Remove(l.id);
                    listeCounts.Add(l.id, oldVoti + l.voti);
                }
                else
                {
                    listeCounts.Add(l.id, l.voti);
                }
            }

            List<RisultatoPartitoWrapped> resWrapped = new List<RisultatoPartitoWrapped>(listeCounts.Keys.Count);
            foreach(var kv in listeCounts)
            {
                if(kv.Value > 0)
                {
                    Partito p = db.GetByPk<Partito>(kv.Key);
                    if(p != null)
                    {
                        var risultato = new RisultatoPartitoWrapped(p, kv.Value);
                        resWrapped.Add(risultato);
                    }
                }
            }
            resWrapped = resWrapped.OrderByDescending(x => x.Voti).ThenBy(x => x.Partito.ordine).ToList();

            Device.BeginInvokeOnMainThread(() =>
            {
                RisultatiListe.AddRange(resWrapped, true);
            });
            
        }
        private void GeneraVotiCandidatiTotale(RisultatiElettorali voti)
        {
            Dictionary<int, int> votiCounts = new Dictionary<int, int>();
            foreach(var c in voti.consiglieri)
            {
                if(votiCounts.ContainsKey(c.id))
                {
                    var oldVoti = votiCounts[c.id];
                    votiCounts.Remove(c.id);
                    votiCounts.Add(c.id, oldVoti + c.voti);
                }
                else
                    votiCounts.Add(c.id, c.voti);
            }
            List<RisultatoCandidatoWrapped> res = new List<RisultatoCandidatoWrapped>();
            foreach(var kv in votiCounts)
            {
                Candidato c = db.GetByPk<Candidato>(kv.Key);
                if(c != null)
                    res.Add(new RisultatoCandidatoWrapped(c, kv.Value));
            }
            res = res.OrderByDescending(x => x.Voti).ThenBy(x => x.Candidato.cognome).ToList();

            Device.BeginInvokeOnMainThread(() =>
            {
                RisultatiCandidati.AddRange(res, true);
            });
        }
        private void GeneraVotiListeSeggio(RisultatiElettorali res, int seggio)
        {
            var liste = res.liste.Where(x => x.seggio == seggio).OrderByDescending(x => x.voti).ThenBy(x => x.ordine).ToList();
            var listeWrapped = new List<RisultatoPartitoWrapped>();
            foreach(var l in liste)
            {
                Partito p = db.GetByPk<Partito>(l.id);
                if(p != null)
                    listeWrapped.Add(new RisultatoPartitoWrapped(p, l.voti));
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                RisultatiListe.AddRange(listeWrapped, true);
            });
        }
        private void GeneraVotiCandidatiSeggio(RisultatiElettorali res, int seggio)
        {
            var candidati = res.consiglieri.Where(x => x.seggio == seggio).OrderByDescending(x => x.voti).ThenBy(x => x.cognome);
            var candidatiWrapped = new List<RisultatoCandidatoWrapped>();
            foreach(var c in candidati)
            {
                Candidato candidato = db.GetByPk<Candidato>(c.id);
                if(candidato != null)
                    candidatiWrapped.Add(new RisultatoCandidatoWrapped(candidato, c.voti));
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                RisultatiCandidati.AddRange(candidatiWrapped, true);
            });
        }
    }

    public class RisultatoPartitoWrapped
    {
        public Partito Partito { get; }
        public int Voti { get; }

        public RisultatoPartitoWrapped(Partito p, int voti)
        {
            Partito = p;
            Voti = voti;
        }
    }
    public class RisultatoCandidatoWrapped
    {
        public Candidato Candidato { get; }
        public int Voti { get; }
        public RisultatoCandidatoWrapped(Candidato c, int v)
        {
            Candidato = c;
            Voti = v;
        }
    }
}
