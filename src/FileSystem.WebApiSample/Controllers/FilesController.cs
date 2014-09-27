namespace Tanka.FileSystem.WebApiSample.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using System.Web.Http;
    using FileSystem.WebApi.FileSystem;
    using FileSystem.WebApi.FlowJS;

    [RoutePrefix("folders")]
    public class FilesController : ApiController
    {
        private readonly Flow _flow;
        private readonly FileSystem _fileSystem;

        public FilesController()
        {
            _fileSystem = new FileSystem
            {
                GetFilePathFunc = filePath => string.Format(
                    "{0}/{1}",
                    HostingEnvironment.MapPath("~/App_Data").Replace("\\", "/"),
                    filePath)
            };

            _flow = new Flow(_fileSystem);
        }

        [Route("{*filePath}")]
        public async Task<HttpResponseMessage> GetFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid path");
            }

            if (!await _fileSystem.ExistsAsync(filePath))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File not found");
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(await _fileSystem.OpenReadAsync(filePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filePath.Substring(filePath.LastIndexOf('/') + 1)
            };

            return response;
        }

        [Route("uploads/{folderName}")]
        public async Task<HttpResponseMessage> Get(string folderName)
        {
            var context = CreateContext(folderName);

            return await _flow.HandleRequest(context);
        }

        [Route("uploads/{folderName}")]
        public async Task<HttpResponseMessage> Post(string folderName)
        {
            var context = CreateContext(folderName);

            return await _flow.HandleRequest(context);
        }

        private FlowRequestContext CreateContext(string folderName)
        {
            return new FlowRequestContext(Request)
            {
                GetChunkFileNameFunc = parameters => string.Format(
                    "{1}_{0}.chunk",
                    parameters.FlowIdentifier,
                    parameters.FlowChunkNumber.Value.ToString().PadLeft(8, '0')),
                GetChunkPathFunc = parameters => string.Format("{0}/chunks/{1}", folderName, parameters.FlowIdentifier),
                GetFileNameFunc = parameters => parameters.FlowFilename,
                GetFilePathFunc = parameters => folderName,
                GetTempFileNameFunc = filePath => string.Format("file_{0}.tmp", Guid.NewGuid()),
                GetTempPathFunc = () => string.Format("{0}/temp", folderName),
                MaxFileSize = ulong.MaxValue
            };
        }
    }
}