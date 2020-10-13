using System.Collections.Generic;

namespace OneSolution.Core.Http
{
    public class ApiClient
    {
        public string BaseUrl { get; set;}
        public string ClientId { get; set; }
        public string ClientKey { get; set; }
    }

    public class ApiClients: List<ApiClient>
    {

    }


    public class ApiClientSettings : Dictionary<string, ApiClient>
    {

    }
}
