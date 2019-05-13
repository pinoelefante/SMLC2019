using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SMLC2019.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UltimiVotiView : ContentView
    {
        public UltimiVotiView()
        {
            InitializeComponent();
        }
        private void ListVoti_Refreshing(object sender, EventArgs e)
        {
            listVoti.IsRefreshing = false;
        }
    }
}