using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pinoelefante.Services
{
    public class WebService
    {
        private HttpClient httpClient;
        private HttpClientHandler handler;
        private CookieContainer cookieContainer;

        public WebService()
        {
            cookieContainer = new CookieContainer();
            handler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true, AllowAutoRedirect = false };
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SMLC2019/1.0");
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        }
        public async Task<string> SendGetRequestAsync(string url, params KeyValuePair<string, string>[] parameters)
        {
            try
            {
                if (parameters?.Length > 0)
                {
                    FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);
                    string urlContent = await content.ReadAsStringAsync();
                    url = $"{url}?{urlContent}";
                }
                CancellationToken token = new CancellationToken();
                var res = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, token);
                if (!token.IsCancellationRequested && res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();
                Debug.WriteLine("WEB - SUCCESS STATUS FAIL");
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine("WEB - EXCEPTION : " + e.Message);
                return null;
            }
        }
        public async Task<string> SendPostRequestAsync(string url, params KeyValuePair<string, string>[] parameters)
        {
            try
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);
                CancellationToken token = new CancellationToken();
                var res = await httpClient.PostAsync(url, content, token);
                if (!token.IsCancellationRequested && res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
        public async Task<string> SendPostJsonRequestAsync(string url, object parameter)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(parameter);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                CancellationToken token = new CancellationToken();
                var res = await httpClient.PostAsync(url, content, token);
                if (!token.IsCancellationRequested && res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
        public async Task<string> SendMultipartPostRequestAsync(string url, byte[] file_content, params KeyValuePair<string, string>[] parameters)
        {
            try
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                if (file_content != null && file_content.Length > 0)
                {
                    var base64content = Convert.ToBase64String(file_content);
                    content.Add(new StringContent(base64content), "file");
                }
                foreach (var kv in parameters)
                    content.Add(new StringContent(kv.Value), kv.Key);

                CancellationToken token = new CancellationToken();
                var res = await httpClient.PostAsync(url, content, token);
                if (!token.IsCancellationRequested && res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
        public async Task<string> SendDeleteRequestAsync(string url, params KeyValuePair<string, string>[] parameters)
        {
            try
            {
                if (parameters.Length > 0)
                {
                    FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);
                    var urlContent = await content.ReadAsStringAsync();
                    url = $"{url}?{urlContent}";
                }
                var token = new CancellationToken();
                var res = await httpClient.DeleteAsync(url, token);
                if (!token.IsCancellationRequested && res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();
                return null;
            }
            catch
            {
                return null;
            }
        }
        public async Task<string> SendRequestAsync(string url, HttpMethod method, object parameters = null, byte[] file = null)
        {
            if(parameters == null || parameters is IEnumerable<KeyValuePair<string,string>>)
            {
                var parameters_conv = (IEnumerable<KeyValuePair<string, string>>)parameters;
                KeyValuePair<string, string>[] parameters_enum = parameters != null ? Enumerable.ToArray(parameters_conv) : null;
                string response = String.Empty;
                switch (method)
                {
                    case HttpMethod.GET:
                        response = await SendGetRequestAsync(url, parameters_enum);
                        break;
                    case HttpMethod.POST:
                        if (file == null)
                            response = await SendPostRequestAsync(url, parameters_enum);
                        else
                            response = await SendMultipartPostRequestAsync(url, file, parameters_enum);
                        break;
                    case HttpMethod.DELETE:
                        response = await SendDeleteRequestAsync(url, parameters_enum);
                        break;
                    case HttpMethod.PUT:
                        // TODO
                        break;
                    case HttpMethod.JSON:
                        response = await SendPostJsonRequestAsync(url, parameters);
                        break;
                }
                if (string.IsNullOrEmpty(response))
                    return null;
                return response;
            }
            else
            {
                string response = await SendPostJsonRequestAsync(url, parameters);
                return response;
            }
        }
        public bool HasCookie(string name, string url)
        {
            return GetCookie(name, url) != null;
        }
        public string GetCookie(string name, string url)
        {
            var cookies = cookieContainer.GetCookies(new Uri(url));
            for(int i = 0; i < cookies.Count; i++)
            {
                if (cookies[i].Name.CompareTo(name) == 0)
                    return cookies[i].Value;
            }
            return null;
        }
        public void SetCookie(string name, string value, string url)
        {
            var uri = new Uri(url);
            Cookie cookie = new Cookie(name, value, "/", uri.Host);
            cookieContainer.Add(cookie);
        }
        public void RemoveCookies(string domain)
        {
            var cookies = cookieContainer.GetCookies(new Uri(domain));
            foreach (Cookie cookie in cookies)
            {
                cookie.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                cookie.Expired = true;
            }
        }
        public void SetHTTPBasicAuthentication(string username, string password)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
        }
    }
    public enum HttpMethod
    {
        GET = 0,
        POST = 1,
        DELETE = 2,
        PUT = 3,
        JSON = 4
    }
}
