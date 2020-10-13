using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using OneSolution.Core.ErrorHandling;

namespace OneSolution.Core.Http
{
    public static class HttpClientExtentions
    {
        public static Task<T> GetAsync<T>(this HttpClient httpClient, string url) where T: class
        {
            return GetAsync<T>(httpClient, url, null, CancellationToken.None);
        }
        public static Task<T> GetAsync<T>(this HttpClient httpClient, string url, NameValueCollection query) where T : class
        {
            return GetAsync<T>(httpClient, url, query, CancellationToken.None);
        }

        public static Task<T> GetAsync<T>(this HttpClient httpClient, string url, NameValueCollection query, CancellationToken cancellationToken) where T : class
        {
            url = Combine(url, query);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return httpClient.SendAsync<T>(request, cancellationToken);
        }
        
        public static Task<T> PostAsync<T>(this HttpClient httpClient, string url, object content) where T : class
        {
            return PostAsync<T>(httpClient, url, null, content, CancellationToken.None);
        }
        public static Task<T> PostAsync<T>(this HttpClient httpClient, string url, NameValueCollection query, object content) where T : class
        {
            return PostAsync<T>(httpClient, url, query, content, CancellationToken.None);
        }

        public static async Task<T> PostAsync<T>(this HttpClient httpClient, string url, NameValueCollection query, object content, CancellationToken cancellationToken) where T : class
        {
            url = Combine(url, query);

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                //request.Content = new JsonContent(content);
                //return await httpClient.SendAsync<T>(request, cancellationToken).ConfigureAwait(false);

                using (var httpContent = await CreateStringContent(content).ConfigureAwait(false))
                {
                    request.Content = httpContent;
                    return await httpClient.SendAsync<T>(request, cancellationToken).ConfigureAwait(false);
                }
            }
            
        }

        public static HttpClient WithBaseUrl(this HttpClient httpClient, string url)
        {
            if (!string.IsNullOrEmpty(url))
                httpClient.BaseAddress = new Uri(url);
            return httpClient;
        }
        public static HttpClient WithConfig(this HttpClient httpClient, ApiClient apiClient)
        {
            httpClient.WithBaseUrl(apiClient.BaseUrl);
            var authInfo = Convert.ToBase64String(Encoding.Default.GetBytes($"{apiClient.ClientId}:{apiClient.ClientKey}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
            return httpClient;
        }

        private static async Task<T> SendAsync<T>(this HttpClient httpClient, HttpRequestMessage request, CancellationToken cancellationToken) where T : class
        {
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (response.Content == null)
            {
                if (response.IsSuccessStatusCode)
                    return null;

                throw new ApiException {
                    StatusCode = response.StatusCode,
                };
            }

            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        return await JsonSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
                    }
                    catch (JsonException je)
                    {
                        //if (stream.CanSeek)
                        //    stream.Seek(0, SeekOrigin.Begin);
                        //var strcontent = await StreamToStringAsync(stream).ConfigureAwait(false);

                        throw new ApiException {
                            StatusCode = HttpStatusCode.InternalServerError,
                            Content = je.ToString() //+ strcontent
                        };
                    }
                }

                var content = await StreamToStringAsync(stream).ConfigureAwait(false);

                throw new ApiException {
                    StatusCode = response.StatusCode,
                    Content = content
                };

            }
        }

        private static async Task<HttpContent> CreateStreamContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var ms = new MemoryStream();
                await JsonSerializer.SerializeAsync(ms, content).ConfigureAwait(false);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                await ms.FlushAsync().ConfigureAwait(false);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }

        private static Task<HttpContent> CreateStringContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                httpContent = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
            }

            return Task.FromResult(httpContent);
        }
        
        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync().ConfigureAwait(false);

            return content;
        }

        private static string Combine(string url, NameValueCollection collection)
        {
            if (collection == null || collection.Count == 0)
                return url;

            return $"{url}?{collection.ToQueryString()}";
        }
    }
}
