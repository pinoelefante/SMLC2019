using System;
using System.Collections.Generic;
using System.Text;
using SMLC2019.Models;
using Xamarin.Forms;
using System.Linq;

namespace SMLC2019.ViewModels
{
    public class AggiungiVoti2ViewModel : AggiungiVoti1ViewModel
    {
        protected override void CaricaCandidati(Partito p)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ElencoCandidatiMaschi.Clear();
                ElencoCandidatiFemmine.Clear();
                if (p != null)
                {
                    var maschi = elencoCandidati[p].Where(x => x.sesso.Equals("M", StringComparison.CurrentCultureIgnoreCase));
                    ElencoCandidatiMaschi.AddRange(maschi);

                    var femmine = elencoCandidati[p].Where(x => x.sesso.Equals("F", StringComparison.CurrentCultureIgnoreCase));
                    ElencoCandidatiFemmine.AddRange(femmine);
                }
            });
        }
    }
}
