using SMLC2019.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SMLC2019.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AggiungiVotoTablet : ContentPage
    {
        public AggiungiVotoTablet()
        {
            InitializeComponent();

            VM.PropertyChanged += VM_PropertyChanged;
        }

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(VM.AltreSchede):
                    PopolaAltreSchede();
                    break;
            }
        }

        private AggiungiVotiTablet VM => this.BindingContext as AggiungiVotiTablet;
        private bool firstStart = true;
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            
            if(firstStart)
            {
                var width = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                var span = (int)Math.Round((width / 130) / 2, MidpointRounding.AwayFromZero);
                collectionMaschi.ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical);
                collectionFemmine.ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical);
            }

            await VM.NavigatedToAsync();
            if (AltreSchedeContainer.Children.Count == 0 && !altreSchedePopolate && !firstStart)
                PopolaAltreSchede();
            firstStart = false;
            
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            var span = (int)((width / 130) / 2);
            collectionMaschi.ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical);
            collectionFemmine.ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical);
        }

        private void CandidatoTapped(object sender, EventArgs e)
        {
            if (e is TappedEventArgs)
            {
                var ev = e as TappedEventArgs;
                var collection = ((sender as Element).Parent as CollectionView);

                if (collection.SelectedItem == ev.Parameter)
                    collection.SelectedItem = null;
                else
                    collection.SelectedItem = ev.Parameter;
            }
        }
        private bool altreSchedePopolate = false;
        private void PopolaAltreSchede()
        {
            lock (this)
            {
                AltreSchedeContainer.Children.Clear();
                if (VM.AltreSchede == null)
                    return;
                for(int i=0;i<VM.AltreSchede.Count;i++)
                {
                    var c = VM.AltreSchede.ElementAt(i);
                    
                    Button b = new Button()
                    {
                        Text = c.cognome
                    };
                    Grid.SetColumn(b, i);
                    Grid.SetRow(b, 0);

                    b.Clicked += (s, e) =>
                    {
                        if (c.sesso.Equals("N", StringComparison.CurrentCultureIgnoreCase) || c.sesso.Equals("M", StringComparison.CurrentCultureIgnoreCase))
                            VM.InserisciAltraScheda(c.partito, c.id, null, c.cognome);
                        else if (c.sesso.Equals("F", StringComparison.CurrentCultureIgnoreCase))
                            VM.InserisciAltraScheda(c.partito, null, c.id, c.cognome);
                    };
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            AltreSchedeContainer.Children.Add(b);
                        }
                        catch { }
                    });
                }
                altreSchedePopolate = true;
            }
            
        }
    }
}