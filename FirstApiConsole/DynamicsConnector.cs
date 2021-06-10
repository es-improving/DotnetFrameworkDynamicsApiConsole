using FirstApiConsole.Entities;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FirstApiConsole
{
    public class DynamicsConnector
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public bool PrettyPrintJson { get; set; }

        private string _userEmail;
        private string _userPassword;
        private string _clientId;
        private string _tenantUrl;

        public DynamicsConnector(string email, string password, string clientId, string tenantUrl)
        {
            _userEmail = email;
            _userPassword = password;
            _clientId = clientId;
            _tenantUrl = tenantUrl;

            var webApiUrl = MakeBaseUrl();
            var authHeader = MakeAuthHeader(webApiUrl);

            _httpClient.BaseAddress = new Uri(webApiUrl);
            _httpClient.DefaultRequestHeaders.Authorization = authHeader;
        }

        private AuthenticationHeaderValue MakeAuthHeader(string webApiUrl)
        {
            var userCredential = new UserCredential(_userEmail, _userPassword);

            var authParameters = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(webApiUrl)).Result;
            var authContext = new AuthenticationContext(authParameters.Authority, false);
            var authResult = authContext.AcquireToken(_tenantUrl, _clientId, userCredential);
            var authHeader = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

            return authHeader;
        }

        private string MakeBaseUrl()
        {
            string apiVersion = "9.2";
            string webApiUrl = $"{_tenantUrl}/api/data/v{apiVersion}/";
            return webApiUrl;
        }

        private void PrettyPrint(string json)
        {
            var jobj = JObject.Parse(json);
            Console.WriteLine(jobj.ToString());
        }

        private string GetResponseString(string uri)
        {

            Console.WriteLine("Sending request (HTTP GET)...", uri);
            Console.WriteLine(uri);

            var response = _httpClient.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Request successful.");

                //Get the response content and parse it.  
                string responseBody = response.Content.ReadAsStringAsync().Result;
                if (PrettyPrintJson)
                {
                    PrettyPrint(responseBody);
                }
                return responseBody;                
            }
            else
            {
                Console.WriteLine("The request failed with a status of '{0}'", response.ReasonPhrase);
                return null;
            }
        }

        public DynamicsResponse<T> Get<T>(string uri)
        {
            var responseBody = GetResponseString(uri);
            return JsonConvert.DeserializeObject<DynamicsResponse<T>>(responseBody);
        }

        public JObject GetJObject(string uri)
        {
            var responseBody = GetResponseString(uri);
            return JObject.Parse(responseBody);
        }

        public JObject GetWithFetchXml(string uri)
        {
            var responseBody = GetResponseString(uri);
            return JObject.Parse(responseBody);
        }

        private HttpRequestMessage CreateHttpRequestMessage(string method, string uri)
        {
            var webApiUrl = MakeBaseUrl();
            var authHeader = MakeAuthHeader(webApiUrl);

            var requestMessage = new HttpRequestMessage(new HttpMethod(method), uri);
            requestMessage.Headers.Authorization = authHeader;

            return requestMessage;
        }

        private void AddJsonContentForRequest(HttpRequestMessage requestMessage, object payload)
        {
            var payloadString = JsonConvert.SerializeObject(payload);

            requestMessage.Content = new StringContent(payloadString);
            requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        }

        public void Patch(string uri, object payload)
        {

            using (var requestMessage = CreateHttpRequestMessage("PATCH", uri))
            {
                AddJsonContentForRequest(requestMessage, payload);

                requestMessage.Headers.Add("Prefer", "return=representation");

                Console.WriteLine("Sending update (HTTP PATCH)...");

                var response = _httpClient.SendAsync(requestMessage).Result;

                string responseBody = response.Content.ReadAsStringAsync().Result;
                if (PrettyPrintJson)
                {
                    Console.WriteLine(responseBody);
                }

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Update successful.");
                }
            }
        }
    }
}
