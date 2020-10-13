using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OneSolution.Core.Http
{
    public static class HttpMessageExtensions
    {
        public static HttpResponseMessage WithContent(this HttpResponseMessage response, object content)
        {
            response.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
            return response;
        }
        public static HttpResponseMessage WithStatus(this HttpResponseMessage response, HttpStatusCode status)
        {
            response.StatusCode = status;
            return response;
        }

        public static HttpRequestMessage WithBasicAuth(this HttpRequestMessage request, ApiClient apiClient)
        {
            var authInfo = Convert.ToBase64String(Encoding.Default.GetBytes($"{apiClient.ClientId}:{apiClient.ClientKey}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
            return request;
        }
    }
}
