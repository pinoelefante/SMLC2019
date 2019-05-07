using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using pinoelefante.Services;
using SMLC2019.Droid;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteDroid))]
namespace SMLC2019.Droid
{
    class SQLiteDroid : ISQLite
    {
        public string DATABASE_PATH { get; private set; }

        public SQLiteDroid()
        {
            var sqliteFilename = "db.sqlite";
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
            DATABASE_PATH = Path.Combine(documentsPath, sqliteFilename);
        }
        
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(DATABASE_PATH);
        }
    }
}