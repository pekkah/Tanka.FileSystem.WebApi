namespace Tanka.WebApi.FileSystem.FlowJS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public class FlowTemporaryFileProvider : MultipartFormDataStreamProvider
    {
        private readonly FlowRequestContext _context;
        private readonly IFileSystem _fileSystem;

        public FlowTemporaryFileProvider(FlowRequestContext context, IFileSystem fileSystem) : base(".")
        {
            _context = context;
            _fileSystem = fileSystem;
            TemporaryFiles = new List<Tuple<string, string>>();
        }

        public List<Tuple<string, string>> TemporaryFiles { get; set; }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (IsFileContent(parent, headers))
            {
                /* is file */
                var fileNames = new Tuple<string, string>(
                    GetLocalFileName(headers),
                    headers.ContentDisposition.FileName);

                TemporaryFiles.Add(fileNames);

                return _fileSystem.OpenWriteAsync(fileNames.Item1).Result;
            }

            /* is form data */
            return new MemoryStream();
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            var path = _context.GetTempPath();
            var fileName = _context.GetTempFileName(headers.ContentDisposition.FileName);
            return string.Concat(path, "/", fileName);
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