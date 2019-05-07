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
    }

    public class DatabaseService
    {
        private ISQLite conMngr;
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
        public void SaveItem<T>(T item)
        {
            using (var conn = GetConnection())
            {
                conn.InsertOrReplace(item);
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
        public void Delete<T>(T item)
        {
            using (var con = GetConnection())
            {
                con.Delete(item);
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
    }
}
