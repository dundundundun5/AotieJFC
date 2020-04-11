using AlgorithmAcceptance.Logging;
using AlgorithmAcceptance.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmAcceptance.Utils
{
    public class HttpClient
    {
        private static WebClient client;
        private static JsonSerializerSettings jsonSettings;
        private ILogger logger;
        private HttpClient()
        {
        }

        private static HttpClient _instance;
        public static HttpClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpClient();
                }
                return _instance;
            }
        }
        public void Init()
        {
            logger = LogManager.CreateLogger("HttpClient");
            jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            client = new WebClient();
            client.Encoding = System.Text.Encoding.GetEncoding("utf-8");
        }
        public T Post<T>(string url, object data = null, Dictionary<string, string> headers = null)
        {
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    client.Headers[item.Key] = item.Value;
                }
            }
            client.Headers[HttpRequestHeader.ContentType] = "application/json";

            // Serialise the data we are sending in to JSON
            string serialisedData = data == null ? string.Empty : JsonConvert.SerializeObject(data, jsonSettings);
            var response = client.UploadString(url, serialisedData);
            logger.Log(LogLevel.Info, $"http post:{url},response" + response);
            return JsonConvert.DeserializeObject<T>(response);
        }


        public T Get<T>(string url, Dictionary<string, string> data = null, Dictionary<string, string> headers = null)
        {
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    // Set the header so it knows we are sending JSON
                    client.Headers[item.Key] = item.Value;
                }
            }
            client.Headers[HttpRequestHeader.ContentType] = "application/json";

            string queryString = "";

            if (data != null)
            {
                foreach (var pair in data)
                {
                    if (queryString.Length != 0)
                    {
                        queryString += "&";
                    }
                    queryString += pair.Key + "=" + pair.Value;
                }
            }

            var response = client.DownloadString(url + "?" + queryString);
            logger.Log(LogLevel.Info, $"http get:{url},response" + response);
            return typeof(T) == typeof(string) ? (T)(object)response : JsonConvert.DeserializeObject<T>(response);
        }
    }
}
