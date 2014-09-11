namespace Tanka.WebApi.FileSystem
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public class StreamMultipartProvider : MultipartFormDataStreamProvider
    {
        private readonly Func<Stream> _getStream;

        public StreamMultipartProvider(Func<Stream> getStream) : base(".")
        {
            _getStream = getStream;
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (IsFileContent(parent, headers))
            {
                return _getStream();
            }

            return new MemoryStream();
        }

        public static bool IsFileContent(HttpContent parent, HttpContentHeaders headers)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            // For form data, Content-Disposition header is a requirement.
            ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
            if (contentDisposition == null)
            {
                // If no Content-Disposition header was present.
                throw new InvalidOperationException("No Content-Disposition header found");
            }

            // The file name's existence indicates it is a file data.
            if (!string.IsNullOrEmpty(contentDisposition.FileName))
            {
                return true;
            }

            return false;
        }
    }
}