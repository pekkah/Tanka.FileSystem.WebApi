namespace Tanka.WebApi.FileSystemSample.Controllers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using System.Web.Http;
    using FileSystem.FileSystem;
    using FileSystem.FlowJS;

    [RoutePrefix("folders")]
    public class FilesController : ApiController
    {
        private readonly Flow _flow;

        public FilesController()
        {
            var fileSystem = new FileSystem
            {
                GetFilePathFunc = filePath => string.Format(
                    "{0}/{1}",
                    HostingEnvironment.MapPath("~/App_Data").Replace("\\", "/"),
                    filePath)
            };

            _flow = new Flow(fileSystem);
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