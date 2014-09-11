namespace Tanka.WebApi.FileSystem.FlowJS
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    public class FlowParametersReader
    {
        public FlowParameters Read(HttpRequestMessage request)
        {
            var query = request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

            return Create(query);
        }

        private FlowParameters Create(Dictionary<string, string> query)
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

        private ulong? Ulong(Dictionary<string, string> values, string key, ulong? defaultValue = null)
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

        private string String(Dictionary<string, string> values, string key, string defaultValue = null)
        {
            string stringValue;

            if (values.TryGetValue(key, out stringValue))
            {
                return stringValue;
            }

            return defaultValue;
        }
    }
}