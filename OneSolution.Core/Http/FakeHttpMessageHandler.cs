using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OneSolution.Core.Http
{
    public class FakeHttpMessageHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage responseMessage;

        public FakeHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            this.responseMessage = responseMessage;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(responseMessage);
        }
    }

}
