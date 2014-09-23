namespace Tanka.FileSystem.WebApi.FlowJS
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Tanka.FileSystem.WebApi;

    public class FlowRequestReader
    {
        public FlowRequest Create(NameValueCollection nameValueCollection)
        {
            Dictionary<string, string> dictionary = nameValueCollection.Cast<string>()
                .Select(s => new {Key = s, Value = nameValueCollection[s]})
                .ToDictionary(p => p.Key, p => p.Value);

            return Create(dictionary);
        }

        public async Task<FlowRequest> ReadGetAsync(HttpRequestMessage request)
        {
            Dictionary<string, string> dictionary = request.GetQueryNameValuePairs()
                .ToDictionary(x => x.Key, x => x.Value);

            return Create(dictionary);
        }

        public async Task<FlowRequest> ReadPostAsync(FlowRequestContext context, IFileSystem fileSystem)
        {
            var provider = new FlowTemporaryFileProvider(context, fileSystem);
            await context.HttpRequest.Content.ReadAsMultipartAsync(provider);

            var flowRequest = Create(provider.FormData);
            flowRequest.TemporaryFile = provider.TemporaryFiles.Single();

            return flowRequest;
        }

        private FlowRequest Create(IDictionary<string, string> query)
        {
            return new FlowRequest
            {
                FlowChunkNumber = Ulong(query, "flowChunkNumber"),
                FlowChunkSize = Ulong(query, "flowChunkSize"),
                FlowFilename = String(query, "flowFilename"),
                FlowIdentifier = String(query, "flowIdentifier"),
                FlowRelativePath = String(query, "flowRelativePath"),
                FlowTotalChunks = Ulong(query, "flowTotalChunks"),
                FlowTotalSize = Ulong(query, "flowTotalSize")
            };
        }

        private string String(IDictionary<string, string> values, string key, string defaultValue = null)
        {
            string stringValue;

            if (values.TryGetValue(key, out stringValue))
            {
                return stringValue;
            }

            return defaultValue;
        }

        private ulong? Ulong(IDictionary<string, string> values, string key, ulong? defaultValue = null)
        {
            string stringValue;

            if (values.TryGetValue(key, out stringValue))
            {
                ulong tempValue;
                if (ulong.TryParse(stringValue, out tempValue))
                {
                    return tempValue;
                }
            }

            return defaultValue;
        }
    }
}