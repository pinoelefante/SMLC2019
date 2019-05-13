using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;

namespace SMLC2019.Services
{
    public class Configuration : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int seggio;
        private string username, password, endpoint, mode = "Tablet";
        private long ultimoInvio;
        private ServerAPI api;
        public int Seggio
        {
            get => seggio;
            set
            {
                Set(ref seggio, value);
                SalvaSeggio();
                Inizializza(true);
            }
        }
        public string Username { get => username; set => Set(ref username, value); }
        public string Password { get => password; set => Set(ref password, value); }
        public string Endpoint { get => endpoint; set => Set(ref endpoint, value); }
        public long UltimoInvio { get => ultimoInvio; set => Set(ref ultimoInvio, value); }
        public string ModalitaVisiva { get => mode; set => Set(ref mode, value); }

        public Configuration(ServerAPI a)
        {
            api = a;
            Inizializza(false);
            LeggiConfigurazione();
            InizializzaAPI();
        }
        private void Inizializza(bool skipSeggio = true)
        {
            if(!skipSeggio)
                Seggio = GetUltimoSeggio();
            UltimoInvio = GetUltimoInvio(Seggio);
        }
        public void InizializzaAPI()
        {
            if(!string.IsNullOrEmpty(Endpoint))
                api.Endpoint = Endpoint;
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                api.SetAuthentication(Username, Password);
        }

        private void LeggiConfigurazione()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "config.json");
            if (!File.Exists(path))
            {
                ModalitaVisiva = "Tablet";
                return;
            }
            var json = File.ReadAllText(path);
            var conf = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Username = conf.ContainsKey(nameof(Username)) ? conf[nameof(Username)] : string.Empty;
            Password = conf.ContainsKey(nameof(Password)) ? conf[nameof(Password)] : string.Empty;
            Endpoint = conf.ContainsKey(nameof(Endpoint)) ? conf[nameof(Endpoint)] : string.Empty;
            ModalitaVisiva = conf.ContainsKey(nameof(ModalitaVisiva)) ? conf[nameof(ModalitaVisiva)] : string.Empty;
        }
        public void SalvaConfigurazione()
        {
            Dictionary<string, string> conf = new Dictionary<string, string>
            {
                { nameof(Username), Username },
                { nameof(Password), Password },
                { nameof(Endpoint), Endpoint },
                { nameof(ModalitaVisiva), ModalitaVisiva }
            };
            var json = JsonConvert.SerializeObject(conf);
            var path = Path.Combine(FileSystem.AppDataDirectory, "config.json");
            try
            {
                File.WriteAllText(path, json);
            }
            catch { }
        }
        private void Set<T>(ref T oldValue, T newValue, [CallerMemberName]string name = "")
        {
            oldValue = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private int GetUltimoSeggio()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "seggio");
            if (!File.Exists(path))
                return 0;
            try
            {
                var text = File.ReadAllText(path);
                return int.Parse(text);
            }
            catch { return 0; }
        }

        private void SalvaSeggio()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "seggio");
            try
            {
                File.WriteAllText(path, Seggio.ToString());
            }
            catch { }
        }

        private long GetUltimoInvio(int seggio)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, seggio.ToString());
            if (!File.Exists(path))
                return 0;
            var text = File.ReadAllText(path);
            try
            {
                return long.Parse(text);
            }
            catch
            {
                return 0;
            }
        }
        public bool SalvaUltimoInvio(int seggio, long tempo)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, seggio.ToString());
            if (seggio == Seggio)
                UltimoInvio = tempo;
            try
            {
                File.WriteAllText(path, tempo.ToString());
                return true;
            }
            catch
            {
                Console.WriteLine("Tempo non salvato. Non chiudere l'applicazione");
                return false;
            }
        }
    }
}
