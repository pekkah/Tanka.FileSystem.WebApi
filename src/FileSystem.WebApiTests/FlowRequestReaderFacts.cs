namespace Tanka.FileSystem.WebApiTests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using FileSystem.WebApi.FileSystem;
    using FileSystem.WebApi.FlowJS;
    using FluentAssertions;
    using Xunit;

    public class FlowRequestReaderFacts
    {
        [Fact]
        public async Task ReadParametersFromGetRequest()
        {
            /* given */
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://files/upload?flowChunkNumber={1}&flowChunkSize={2048}&flowTotalSize={1024}&flowIdentifier=0001-my-file.file&flowFilename=my-file.file", UriKind.Absolute)
            };

            var reader = new FlowRequestReader();

            /* when */
            var parameters = await reader.ReadGetAsync(httpRequest).ConfigureAwait(false);

            /* then */
            parameters.FlowChunkNumber.ShouldBeEquivalentTo(1);
            parameters.FlowChunkSize.ShouldBeEquivalentTo(2048);
            parameters.FlowTotalSize.ShouldBeEquivalentTo(1024);
            parameters.FlowIdentifier.ShouldBeEquivalentTo("0001-my-file.file");
            parameters.FlowFilename.ShouldBeEquivalentTo("my-file.file");
        }

        [Fact(Skip = "Requires refactoring as there are now dependencies to file system and context")]
        public async Task ReadParametersFromPostRequest()
        {
            /* given */
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://files/upload")
            };

            var streamContent = new StreamContent(new MemoryStream());
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "my-file.txt"
            };

            var formDataContent = new MultipartFormDataContent
            {
                {new StringContent("1"), "flowChunkNumber"},
                {new StringContent("2048"), "flowChunkSize"},
                {new StringContent("1024"), "flowTotalSize"},
                {new StringContent("0001-my-file.file"), "flowIdentifier"},
                {new StringContent("my-file.file"), "flowFilename"},
                {streamContent, "content", "my-file.txt"}
            };
            var context = new FlowRequestContext(httpRequest);

            /* when */
            using (formDataContent)
            {
                httpRequest.Content = formDataContent;

                var reader = new FlowRequestReader();
                FlowRequest request;
                using (var memoryStream = new MemoryStream())
                {
                    request =
                        await reader.ReadPostAsync(context, new FileSystem()).ConfigureAwait(false);
                }

                /* then */
                request.FlowChunkNumber.ShouldBeEquivalentTo(1);
                request.FlowChunkSize.ShouldBeEquivalentTo(2048);
                request.FlowTotalSize.ShouldBeEquivalentTo(1024);
                request.FlowIdentifier.ShouldBeEquivalentTo("0001-my-file.file");
                request.FlowFilename.ShouldBeEquivalentTo("my-file.file");
            }
        }
    }
}