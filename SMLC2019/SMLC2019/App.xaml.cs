﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SMLC2019.Services;
using SMLC2019.Views;

namespace SMLC2019
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
