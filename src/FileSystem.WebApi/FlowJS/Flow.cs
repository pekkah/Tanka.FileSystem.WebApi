namespace Tanka.FileSystem.WebApi.FlowJS
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Tanka.FileSystem.WebApi;

    public class Flow
    {
        private readonly IFileSystem _fileSystem;
        private readonly FlowRequestReader _requestReader;

        public Flow(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _requestReader = new FlowRequestReader();
        }

        public async Task<HttpResponseMessage> HandleRequest(FlowRequestContext context)
        {
            if (context.HttpRequest.Method == HttpMethod.Get)
            {
                return await HandleGetRequest(context).ConfigureAwait(false);
            }

            if (context.HttpRequest.Method != HttpMethod.Post)
            {
                return context.HttpRequest.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Only GET and POST requests supported");
            }

            return await HandlePostRequest(context).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> HandleGetRequest(FlowRequestContext context)
        {
            FlowRequest request = await _requestReader.ReadGetAsync(context.HttpRequest).ConfigureAwait(false);

            if (!IsValidRequest(context, request))
            {
                return context.HttpRequest.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Invalid flow GET request");
            }

            string filePath = GetChunkFilePath(context, request);

            if (!await _fileSystem.ExistsAsync(filePath).ConfigureAwait(false))
            {
                return context.HttpRequest.CreateErrorResponse(
                    HttpStatusCode.NoContent,
                    "File not found");
            }

            return context.HttpRequest.CreateResponse(HttpStatusCode.OK);
        }

        private static string GetChunkFilePath(FlowRequestContext context, FlowRequest request)
        {
            return string.Concat(
                context.GetChunkPathFunc(request),
                "/",
                context.GetChunkFileName(request));
        }

        private async Task<HttpResponseMessage> HandlePostRequest(FlowRequestContext context)
        {
            // read request 
            FlowRequest request = await _requestReader.ReadPostAsync(
                context,
                _fileSystem).ConfigureAwait(false);

            // is valid request?
            if (!IsValidRequest(context, request))
            {
                await _fileSystem.DeleteAsync(request.TemporaryFile.Item1).ConfigureAwait(false);
                return context.HttpRequest.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Invalid flow POST request");
            }

            // upload temporary to chunks
            var chunkFilePath = GetChunkFilePath(context, request);

            using (Stream chunkStream = await _fileSystem.OpenWriteAsync(chunkFilePath).ConfigureAwait(false))
            {
                using (Stream tempStream = await _fileSystem.OpenReadAsync(request.TemporaryFile.Item1).ConfigureAwait(false))
                {
                    await tempStream.CopyToAsync(chunkStream).ConfigureAwait(false);
                }
            }

            // delete temporary
            await _fileSystem.DeleteAsync(request.TemporaryFile.Item1).ConfigureAwait(false);

            // if last chunk combine and move to files
            if (request.IsLastChunk)
            {
                await CombineAsync(context, request).ConfigureAwait(false);
            }

            return context.HttpRequest.CreateResponse(HttpStatusCode.OK);
        }

        private async Task CombineAsync(FlowRequestContext context, FlowRequest request)
        {
            var filePath = string.Concat(
                context.GetFilePathFunc(request),
                "/",
                context.GetFileName(request));

            var chunkPath = context.GetChunkPathFunc(request);
            var chunkFilePaths = await _fileSystem.ListDirectoryAsync(chunkPath).ConfigureAwait(false);

            using (var fileStream = await _fileSystem.OpenWriteAsync(filePath).ConfigureAwait(false))
            {
                foreach (var file in chunkFilePaths)
                {
                    using (var sourceStream = await _fileSystem.OpenReadAsync(file).ConfigureAwait(false))
                    {
                        await sourceStream.CopyToAsync(fileStream).ConfigureAwait(false);
                    }
                }

                await fileStream.FlushAsync().ConfigureAwait(false);
            }

            await _fileSystem.DeleteDirectoryAsync(chunkPath).ConfigureAwait(false);
        }

        private bool IsValidRequest(FlowRequestContext context, FlowRequest request)
        {
            if (!request.FlowChunkNumber.HasValue || 
                !request.FlowChunkSize.HasValue ||
                !request.FlowTotalSize.HasValue)
            {
                return false;
            }

            ulong chunkNumber = request.FlowChunkNumber.Value;
            ulong chunkSize = request.FlowChunkSize.Value;
            ulong totalSize = request.FlowTotalSize.Value;


            if (chunkNumber == 0 || chunkSize == 0 || totalSize == 0)
            {
                return false;
            }

            double numberOfChunks =
                Math.Max(Math.Floor(request.FlowTotalSize.Value/(request.FlowChunkSize.Value*1.0)), 1);

            if (chunkNumber > numberOfChunks)
            {
                return false;
            }

            if (totalSize > context.MaxFileSize)
            {
                return false;
            }

            if (chunkSize > context.MaxFileSize)
            {
                return false;
            }

            return true;
        }
    }
}