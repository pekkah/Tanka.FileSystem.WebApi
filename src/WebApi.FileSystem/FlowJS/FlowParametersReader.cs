namespace Tanka.WebApi.FileSystem.FlowJS
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class FlowParametersReader
    {
        public FlowParameters Create(NameValueCollection nameValueCollection)
        {
            Dictionary<string, string> dictionary = nameValueCollection.Cast<string>()
                .Select(s => new {Key = s, Value = nameValueCollection[s]})
                .ToDictionary(p => p.Key, p => p.Value);

            return Create(dictionary);
        }

        public async Task<FlowParameters> ReadGetAsync(HttpRequestMessage request)
        {
            Dictionary<string, string> dictionary = request.GetQueryNameValuePairs()
                .ToDictionary(x => x.Key, x => x.Value);

            return Create(dictionary);
        }

        public async Task<FlowParameters> ReadPostAsync<T>(HttpRequestMessage request, T streamProvider) where T : MultipartFormDataStreamProvider
        {
            await request.Content.ReadAsMultipartAsync(streamProvider);

            return Create(streamProvider.FormData);
        }

        private FlowParameters Create(IDictionary<string, string> query)
        {
            return new FlowParameters
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