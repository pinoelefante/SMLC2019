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
            var response = await SendRequestAsync<Assets>($"{Endpoint}?action=GetAssets", HttpMethod.GET);
            return response == null ? null : response.content;
        }
        
        public async Task GetVotiPerSeggioAsync()
        {
            var response = await SendRequestAsync<Assets>($"{Endpoint}?action=GetAssets", HttpMethod.GET);
        }

        public void GetVoti(int seggio)
        {

        }

        public async Task<bool> AggiungiVotiAsync(List<Voto> voti)
        {
            var response = await SendRequestAsync<bool>($"{Endpoint}?action=AggiungiVoti", HttpMethod.JSON, voti);
            if (response == null)
                return false;
            return response.content;
        }

        public void SetAuthentication(string username, string password)
        {
            webservice.SetHTTPBasicAuthentication(username, password);
        }

        private async Task<RootMessage<T>> SendRequestAsync<T>(string url, HttpMethod method, object parameters = null, byte[] file = null, bool send_later = false)
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
        public List<Partito> partiti { get; set; }
        public List<Candidato> candidati { get; set; }
    }

    public class RootMessage<Content>
    {
        public string time { get; set; }
        public Content content { get; set; }
    }
}
