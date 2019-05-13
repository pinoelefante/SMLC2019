using Newtonsoft.Json;
using pinoelefante.Services;
using SMLC2019.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SMLC2019.Services
{
    public class ServerAPI
    {
        public string Endpoint { get; set; }

        public WebService webservice;

        public ServerAPI()
        {
            webservice = new WebService();
        }

        public async Task<Assets> GetAssetsAsync()
        {
            var response = await SendRequestAsync<Assets>($"{Endpoint}/endpoint.php?action=GetAssets", HttpMethod.GET);
            return response?.Content;
        }
        
        public async Task<RisultatiElettorali> GetVotiPerSeggioAsync()
        {
            var response = await SendRequestAsync<RisultatiElettorali>($"{Endpoint}/endpoint.php?action=RisultatiPerSeggio", HttpMethod.GET);
            return response?.Content;
        }

        public void GetVoti(int seggio)
        {

        }

        public async Task<bool> AggiungiVotiAsync(List<Voto> voti)
        {
            var response = await SendRequestAsync<bool>($"{Endpoint}/endpoint.php?action=AggiungiVoti", HttpMethod.JSON, voti);
            return response == null ? false : response.Content;
        }

        public async Task<bool> Ping()
        {
            var res = await SendRequestAsync<bool>($"{Endpoint}/endpoint.php?action=Ping", HttpMethod.GET);
            return res == null ? false : res.Content;
        }

        public async Task<bool> CancellaVoto(int seggio, long tempo)
        {
            var p = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("seggio", seggio.ToString()),
                new KeyValuePair<string, string>("tempo", tempo.ToString()),
            };
            var res = await SendRequestAsync<bool>($"{Endpoint}/endpoint.php?action=CancellaVoto", HttpMethod.POST, p);
            return res == null ? false : res.Content;
        }

        public void SetAuthentication(string username, string password)
        {
            webservice.SetHTTPBasicAuthentication(username, password);
        }

        private async Task<RootMessage<T>> SendRequestAsync<T>(string url, HttpMethod method, object parameters = null, byte[] file = null)
        {
            
            var res = await webservice.SendRequestAsync(url, method, parameters, file);

            if (string.IsNullOrEmpty(res))
                return null;

            try
            {
                Debug.WriteLine("JSON: " + res);
                return JsonConvert.DeserializeObject<RootMessage<T>>(res);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
            
        }
    }
    
    public class Assets
    {
        [JsonProperty(PropertyName = "partiti")]
        public List<Partito> Partiti { get; set; }
        [JsonProperty(PropertyName = "candidati")]
        public List<Candidato> Candidati { get; set; }
    }

    public class RootMessage<ContentType>
    {
        [JsonProperty(PropertyName = "time")]
        public string Time { get; set; }
        [JsonProperty(PropertyName = "content")]
        public ContentType Content { get; set; }
    }

    public class Consigliere
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string cognome { get; set; }
        public string sesso { get; set; }
        public int partito { get; set; }
        public string foto { get; set; }
        public int seggio { get; set; }
        public int voti { get; set; }
        public string nome_partito { get; set; }
        public string logo { get; set; }
    }

    public class ListaPolitica
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string sindaco { get; set; }
        public string sindaco_foto { get; set; }
        public string logo { get; set; }
        public int ordine { get; set; }
        public int seggio { get; set; }
        public int voti { get; set; }
    }

    public class RisultatiElettorali
    {
        public List<Consigliere> consiglieri { get; set; }
        public List<ListaPolitica> liste { get; set; }
    }
}
