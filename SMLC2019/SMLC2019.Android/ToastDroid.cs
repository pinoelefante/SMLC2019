using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.CurrentActivity;
using SMLC2019.Droid;
using SMLC2019.Services;
using Xamarin.Forms;

[assembly:Dependency(typeof(ToastDroid))]
namespace SMLC2019.Droid
{
    public class ToastDroid : IToast
    {
        public void ShowToast(string message)
        {
            var activity = CrossCurrentActivity.Current.Activity;
            Toast.MakeText(activity, message, ToastLength.Short).Show();
        }
    }
}