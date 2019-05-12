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
        public RelayCommand CaricaVotiCommand => caricaVotiCmd ?? (caricaVotiCmd = new RelayCommand(CaricaVoti));
        public ObservableCollection<RisultatoPartitoWrapped> RisultatiListe { get; } = new ObservableCollection<RisultatoPartitoWrapped>();
        

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
                GeneraVotiListeTotale(res);
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
            resWrapped = resWrapped.OrderByDescending(x => x.Voti).ToList();

            Device.BeginInvokeOnMainThread(() =>
            {
                RisultatiListe.AddRange(resWrapped, true);
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
}
