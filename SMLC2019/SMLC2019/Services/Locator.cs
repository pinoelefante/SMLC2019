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
            SimpleIoc.Default.Register<Configuration>();
            SimpleIoc.Default.Register(() => DependencyService.Get<IToast>());
            RegisterViewModels();
        }

        static void RegisterViewModels()
        {
            SimpleIoc.Default.Register<AggiungiVotiSmartphone>();
            SimpleIoc.Default.Register<AggiungiVotiTablet>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public static T GetService<T>() => SimpleIoc.Default.GetInstance<T>();

        public AggiungiVotiSmartphone AggiungiVotiSmartphone => GetService<AggiungiVotiSmartphone>();
        public AggiungiVotiTablet AggiungiVotiTablet => GetService<AggiungiVotiTablet>();
        public SettingsViewModel SettingsViewModel => GetService<SettingsViewModel>();
    }
}
