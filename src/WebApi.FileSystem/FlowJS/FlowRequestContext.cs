namespace Tanka.WebApi.FileSystem.FlowJS
{
    using System;
    using System.Net.Http;

    public class FlowRequestContext
    {
        private readonly HttpRequestMessage _httpRequest;

        public FlowRequestContext(HttpRequestMessage httpRequest)
        {
            _httpRequest = httpRequest;
            GetFileNameFunc = parameters => "";
            GetChunkFileNameFunc = parameters => "";
            GetTempFileNameFunc = fileName => string.Format("file_{0}", Guid.NewGuid());
            GetTempPathFunc = () => "temp";
        }

        public HttpRequestMessage HttpRequest
        {
            get { return _httpRequest; }
        }

        public Func<FlowRequest, string> GetFileNameFunc { get; set; }

        public Func<FlowRequest, string> GetChunkFileNameFunc { get; set; }

        public Func<string, string> GetTempFileNameFunc { get; set; }

        public Func<string> GetTempPathFunc { get; set; } 

        public Func<FlowRequest, string> GetChunkPathFunc { get; set; }

        public Func<FlowRequest, string> GetFilePathFunc { get; set; }

        public ulong MaxFileSize { get; set; }

        public string TempFileBasePath { get; set; }

        public string GetChunkPath(FlowRequest request)
        {
            if (GetChunkPathFunc == null)
            {
                throw new InvalidOperationException("GetChunkPathFunc is null. Set it to valid function.");
            }

            return GetChunkPathFunc(request);
        }

        public string GetChunkFileName(FlowRequest request)
        {
            if (GetChunkFileNameFunc == null)
            {
                throw new InvalidOperationException("GetChunkFileNameFunc is null. Set it to valid function.");
            }

            return GetChunkFileNameFunc(request);
        }


        public string GetFileName(FlowRequest request)
        {
            if (GetFileNameFunc == null)
            {
                throw new InvalidOperationException("GetFileNameFunc not set. Set it to valid function.");
            }

            return GetFileNameFunc(request);
        }

        public string GetFilePath(FlowRequest request)
        {
            if (GetFilePathFunc == null)
            {
                throw new InvalidOperationException("GetFilePathFunc not set. Set it to valid function.");
            }

            return GetFilePathFunc(request);
        }

        public string GetTempFileName(string fileName)
        {
            if (GetTempFileNameFunc == null)
            {
                throw new InvalidOperationException("GetTempFileNameFunc not set. Set it to valid function.");
            }

            return GetTempFileNameFunc(fileName);
        }

        public string GetTempPath()
        {
            if (GetTempPathFunc == null)
            {
                throw new InvalidOperationException("GetTempPathFunc not set. Set it to valid function.");
            }

            return GetTempPathFunc();
        }
    }
}