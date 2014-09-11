namespace Tanka.WebApi.FileSystemSample.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;
    using FileSystem.FlowJS;

    [RoutePrefix("files")]
    public class FilesController : ApiController
    {
        private readonly FlowParametersReader _parameterReader;

        public FilesController()
        {
            _parameterReader = new FlowParametersReader();
        }

        [Route()]
        public HttpResponseMessage Get()
        {
            var parameters = _parameterReader.Read(Request);


            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [Route("folders/{folderName}")]
        public HttpResponseMessage Post()
        {
            var parameters = _parameterReader.Read(Request);
        }
    }
}