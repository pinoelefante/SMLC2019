using pinoelefante.Services;
using SMLC2019.Models;
using SMLC2019.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace pinoelefante.Services
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
        void DeleteDatabaseFile();
    }

    public class DatabaseService
    {
        private readonly ISQLite conMngr;
        public DatabaseService(ISQLite sqlite)
        {
            conMngr = sqlite;
            CreateDatabase();
        }
        private void CreateDatabase()
        {
            using (var con = GetConnection())
            {
                con.CreateTables(CreateFlags.None, typeof(Partito), typeof(Candidato), typeof(Voto));
            }
        }
        private SQLiteConnection GetConnection()
        {
            return conMngr.GetConnection();
        }
        public void DeleteDatabase()
        {
            using (var con = GetConnection())
            {
                con.DropTable<Voto>();
                con.DropTable<Candidato>();
                con.DropTable<Partito>();
            }
            CreateDatabase();
        }
        public void TruncateTable<T>()
        {
            using(var conn = GetConnection())
            {
                conn.DeleteAll<T>();
            }
        }
        public bool SaveItem<T>(T item)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    return conn.Insert(item) > 0;
                }
                catch
                {
                    return false;
                }
            }
        }
        public void SaveItems<T>(IEnumerable<T> list)
        {
            using (var con = GetConnection())
            {
                con.RunInTransaction(() =>
                {
                    foreach (T item in list)
                        con.InsertOrReplace(item);
                });
            }
        }
        public bool Delete<T>(T item)
        {
            using (var con = GetConnection())
            {
                return con.Delete(item) > 0;
            }
        }
        
        public T GetByPk<T>(object pk)
        {
            try
            {
                using (var con = GetConnection())
                {
                    var mapping = con.GetMapping<T>();
                    return (T)con.Get(pk, mapping);
                }
            }
            catch
            {
                return default(T);
            }
        }
        public bool DeleteByPk<T>(object pk)
        {
            using (var con = GetConnection())
            {
                var mapping = con.GetMapping<T>();
                return con.Delete(pk, mapping) > 0;
            }
        }

        public IEnumerable<Partito> GetAllPartiti()
        {
            using (var con = GetConnection())
            {
                return con.Table<Partito>().ToList();
            }
        }
        public IEnumerable<Candidato> GetAllCandidati()
        {
            using (var con = GetConnection())
            {
                return con.Table<Candidato>().ToList();
            }
        }
        public IEnumerable<Voto> GetVotiBySeggio(int seggio)
        {
            using (var con = GetConnection())
            {
                return con.Table<Voto>().Where(x => x.seggio == seggio).ToList();
            }
        }
        public IEnumerable<Voto> GetAllVoti()
        {
            using (var con = GetConnection())
            {
                return con.Table<Voto>().ToList();
            }
        }

        public IEnumerable<Voto> GetLastVoti(int seggio, int limit=5)
        {
            using (var con = GetConnection())
            {
                return con.Table<Voto>().Where(x => x.seggio == seggio)
                                        .OrderByDescending(x => x.tempo)
                                        .Take(limit)
                                        .ToList();
            }
        }
        public List<Voto> GetVotiDaCaricare(int seggio, long last)
        {
            using(var conn = GetConnection())
            {
                return conn.Table<Voto>().Where(x => x.seggio == seggio && x.tempo > last).ToList();
            }
        }

        public int GetVotiDaCaricareCount(int seggio, long last)
        {
            using(var conn = GetConnection())
            {
                return conn.Table<Voto>().Where(x => x.seggio == seggio && x.tempo > last).Count();
            }
        }

        public int GetVotiPartitoCount(int seggio, int partito)
        {
            using(var conn = GetConnection())
            {
                return conn.Table<Voto>().Where(x => x.seggio == seggio && x.partito == partito).Count();
            }
        }
    }
}
