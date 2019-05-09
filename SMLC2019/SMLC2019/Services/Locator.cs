using GalaSoft.MvvmLight.Ioc;
using pinoelefante.Services;
using SMLC2019.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SMLC2019.Services
{
    public class Locator
    {
        static Locator()
        {
            SimpleIoc.Default.Register(() => DependencyService.Get<ISQLite>());
            SimpleIoc.Default.Register<ServerAPI>();
            SimpleIoc.Default.Register<DatabaseService>();

            RegisterViewModels();
        }

        static void RegisterViewModels()
        {
            SimpleIoc.Default.Register<AggiungiVoti1ViewModel>();
            SimpleIoc.Default.Register<AggiungiVoti2ViewModel>();
        }

        public static T GetService<T>() => SimpleIoc.Default.GetInstance<T>();

        public AggiungiVoti1ViewModel AggiungiVoti1ViewModel => GetService<AggiungiVoti1ViewModel>();
        public AggiungiVoti2ViewModel AggiungiVoti2ViewModel => GetService<AggiungiVoti2ViewModel>();
    }
}
