using System;
using System.Net.Http;

namespace OneSolution.Core.Http
{
    public class FakeHttpClient: HttpClient
    {
        public FakeHttpClient(FakeHttpMessageHandler handler): base(handler)
        {
            BaseAddress = new Uri("http://localhost");
        }

        public FakeHttpClient(FakeHttpMessageHandler handler, string baseUrl) : base(handler)
        {
            BaseAddress = new Uri(baseUrl);
        }
    }
}
