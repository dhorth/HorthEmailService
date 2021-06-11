using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared;
using Horth.Service.Email.Shared.Exceptions;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Serilog;

namespace Horth.Service.Email.Shared.Service
{
    public abstract class ServiceNameEnumeration : IComparable
    {
        public string Name { get; private set; }


        protected ServiceNameEnumeration(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;

        public static IEnumerable<T> GetAll<T>() where T : ServiceNameEnumeration
        {
            var fields = typeof(T).GetFields(BindingFlags.Public |
                                             BindingFlags.Static |
                                             BindingFlags.DeclaredOnly);

            return fields.Select(f => f.GetValue(null)).Cast<T>();
        }

        public override bool Equals(object obj)
        {
            var otherValue = obj as ServiceNameEnumeration;

            if (otherValue == null)
                return false;

            var typeMatches = GetType() == obj.GetType();
            var valueMatches = Name.Equals(otherValue.Name);

            return typeMatches && valueMatches;
        }

        //here to avoid warning
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object other) => String.Compare(Name, ((ServiceNameEnumeration)other).Name, StringComparison.Ordinal);

        // Other utility methods ...
    }
    public class ServiceName : ServiceNameEnumeration
    {
        public ServiceName(string name)
            : base(name)
        {
        }
    }
    public class IrcBaseService
    {
        private static readonly object Lock = new object();
        public IrcBaseService(AppSettings appSettings)
        {
            AppSettings = appSettings;
        }

        public virtual AppSettings AppSettings { get; set; }
        public ILogger Logger => Log.Logger;

        protected static object LockObj => Lock;

    }

    public class IrcService : IrcBaseService, IDisposable
    {
        private static readonly HttpClientHandler ClientHandler = new HttpClientHandler();
        //private readonly HttpClient _apiClient;
        protected string _baseUrl;


        public IrcService(AppSettings appSettings) : base(appSettings)
        {
        }

        private AsyncRetryPolicy<HttpResponseMessage> GetPolicy()
        {
            var jitterer = new Random();
            var ret = Polly.Policy.HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(Math.Pow(2, i))
                       + TimeSpan.FromMilliseconds(jitterer.Next(1, 2000)),
                       (result, timeSpan, retryCount, context) =>
                {
                    Log.Logger.Warning($"Request({result.Result.RequestMessage?.RequestUri}) failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                })
                ;
            return ret;
        }
        private async Task<HttpResponseMessage> GetResponse(Uri uri)
        {
            using (var apiClient = new HttpClient(ClientHandler, false))
            {
                apiClient.Timeout = TimeSpan.FromMinutes(5);
                var ret = await GetPolicy().ExecuteAsync(() => apiClient.GetAsync(uri));
                return ret;
            }
        }
        private async Task<HttpResponseMessage> PostRequest(Uri uri, object data)
        {
            using (var apiClient = new HttpClient(ClientHandler, false))
            {
                var ret = await GetPolicy().ExecuteAsync(() => apiClient.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")));
                return ret;
            }
        }


        protected async Task<List<T>> GetList<T>(ServiceName serviceName, params string[] query)
        {
            var ret = new List<T>();
            var url = MakeUrl(serviceName, query);
            var uri = new Uri(url);

            Log.Logger.Debug($"GetList({url})");
            try
            {
                using (var response = await GetResponse(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseString))
                        {
                            ret = JsonConvert.DeserializeObject<List<T>>(responseString);
                        }
                    }
                    else
                    {
                        Log.Logger.Error($"Request Failed: Code:{response.StatusCode} Reason: {response.ReasonPhrase} Url:{url}");
                        throw new IrcWebException(response, serviceName, url);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, url);
            }
            return ret;
        }
        protected async Task<T> GetObject<T>(ServiceName serviceName, params string[] query)
        {
            var ret = default(T);
            var url = MakeUrl(serviceName, query);
            var uri = new Uri(url);
            Log.Logger.Debug($"GetObject({url})");
            try
            {
                using (var response = await GetResponse(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseString))
                        {
                            ret = JsonConvert.DeserializeObject<T>(responseString,
                                new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
                        }
                    }
                    else
                    {
                        Log.Logger.Error($"Request Failed: Code:{response.StatusCode} Reason: {response.ReasonPhrase} Url:{url}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, url);
            }
            return ret;
        }
        protected async Task<T> Post<T>(ServiceName serviceName, params string[] query)
        {
            var ret = default(T);
            string url = serviceName.ToString();
            try
            {
                url = MakeUrl(serviceName, query);
                var uri = new Uri(url);

                Log.Logger.Information($"Execute({url})");
                // var content = new StringContent(url, UnicodeEncoding.UTF8, "application/json");
                using (var response = await PostRequest(uri, url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var str = await response.Content.ReadAsStringAsync();
                        ret = JsonConvert.DeserializeObject<T>(str);
                    }
                    else
                    {
                        Log.Logger.Error($"Request Failed: Code:{response.StatusCode} Reason: {response.ReasonPhrase} Url:{url}");
                        throw new IrcWebException(response, serviceName, url);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, url);
            }
            return ret;
        }
        protected async Task<bool> Execute(ServiceName serviceName, params string[] query)
        {
            bool rc = false;
            string url = serviceName.ToString();
            try
            {
                url = MakeUrl(serviceName, query);
                var uri = new Uri(url);

                Log.Logger.Information($"Execute({url})");
                //var content = new StringContent(url, UnicodeEncoding.UTF8, "application/json");
                using (var response = await PostRequest(uri, url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        rc = true;
                    }
                    else
                    {
                        Log.Logger.Error($"Request Failed: Code:{response.StatusCode} Reason: {response.ReasonPhrase} Url:{url}");
                        throw new IrcWebException(response, serviceName, url);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, url);
            }
            return rc;
        }
        protected async Task<T> Execute<T>(ServiceName serviceName, object data = null)
        {
            var ret = default(T);
            string url = serviceName.ToString();
            try
            {
                //curl -X POST "https://localhost:5001/api/v1/AddTask" -H  "accept: text/plain"
                url = MakeUrl(serviceName, "");
                //posting objects requires a trailing slash on the url
                if (data != null)
                    url += "/";

                var uri = new Uri(url);

                Log.Logger.Information($"Execute({url})");
                if (data == null)
                    data = "";
                // var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                using (var response = await PostRequest(uri, data))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var str = await response.Content.ReadAsStringAsync();
                        ret = JsonConvert.DeserializeObject<T>(str);
                    }
                    else
                    {
                        Log.Logger.Error($"Request Failed: Code:{response.StatusCode} Reason: {response.ReasonPhrase} Url:{url}");
                        throw new IrcWebException(response, serviceName, url);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, url);
                throw new IrcWebException(null, serviceName, url);
            }
            return ret;
        }
        //protected async Task<List<DataTableTabData>> GetTabData(ServiceName serviceName, params string[] query)
        //{
        //    var ret = new List<DataTableTabData>();
        //    var url = MakeUrl(serviceName, query);
        //    var uri = new Uri(url);

        //    Log.Logger.Debug($"GetList({url})");
        //    try
        //    {
        //        using (var response = await GetResponse(uri))
        //        {
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var responseString = await response.Content.ReadAsStringAsync();
        //                if (!string.IsNullOrEmpty(responseString))
        //                {
        //                    ret = JsonConvert.DeserializeObject<List<DataTableTabData>>(responseString);
        //                }
        //            }
        //            else
        //            {
        //                Log.Logger.Error($"Request Failed: Code:{response.StatusCode} Reason: {response.ReasonPhrase} Url:{url}");
        //                throw new IrcWebException(response, serviceName, url);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Error(ex, url);
        //    }
        //    return ret;
        //}
        //protected async Task<DataTableResult<T>> GetTable<T>(ServiceName serviceName, DTParameters dt)
        //{
        //    var ret = default(DataTableResult<T>);
        //    string url = serviceName.ToString();
        //    try
        //    {
        //        //curl -X POST "https://localhost:5001/api/v1/AddTask" -H  "accept: text/plain"
        //        url = MakeUrl(serviceName, "");
        //        //posting objects requires a trailing slash on the url
        //        url += "/";

        //        var uri = new Uri(url);

        //        Log.Logger.Information($"Execute({url})");
        //        // var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        //        using (var response = await PostRequest(uri, dt))
        //        {
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var str = await response.Content.ReadAsStringAsync();
        //                ret = JsonConvert.DeserializeObject<DataTableResult<T>>(str);
        //            }
        //            else
        //            {
        //                Log.Logger.Error($"Request Failed: Code:{response.StatusCode} Reason: {response.ReasonPhrase} Url:{url}");
        //                throw new IrcWebException(response, serviceName, url);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Error(ex, url);
        //    }
        //    return ret;
        //}

        public virtual void Dispose()
        {
            //_clientHandler?.Dispose();
        }


        protected string MakeUrl(ServiceName serviceName, params string[] query)
        {
            var url = $"{_baseUrl}/{serviceName}";
            if (query.Length > 0)
            {
                url += "?";
                foreach (var q in query)
                {
                    url += q + "&";
                }
                url = url.TrimEnd('&');
            }
            if (url.EndsWith("?"))
                url = url.TrimEnd('?');

            return url;
        }

    }
}
