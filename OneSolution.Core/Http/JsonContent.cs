using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Net;

namespace OneSolution.Core.Http
{
    public class JsonContent : HttpContent
    {
        public object SerializationTarget { get; private set; }
        public JsonContent(object serializationTarget)
        {
            SerializationTarget = serializationTarget;
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
        protected override async Task SerializeToStreamAsync(Stream stream,
                                                    TransportContext context)
        {
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
                await JsonSerializer.SerializeAsync(stream, SerializationTarget).ConfigureAwait(false);

        }

        protected override bool TryComputeLength(out long length)
        {
            //we don't know. can't be computed up-front
            length = -1;
            return false;
        }
    }
}
