using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Foundation;
using pinoelefante.Services;
using SQLite;
using UIKit;

namespace SMLC2019.iOS
{
    public class SQLiteApple : ISQLite
    {
        private string DB_PATH;

        public SQLiteApple()
        {
            var sqliteFilename = "db.sqlite";
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // Documents folder
            string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
            DB_PATH = Path.Combine(libraryPath, sqliteFilename);
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(DB_PATH);
        }
        public void DeleteDatabaseFile()
        {
            try
            {
                File.Delete(DB_PATH);
                Console.WriteLine("Database deleted");
            }
            catch
            {
                Console.WriteLine("Database not deleted");
            }
        }
    }
}