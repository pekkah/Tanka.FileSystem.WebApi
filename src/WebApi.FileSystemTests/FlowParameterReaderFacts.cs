namespace Tanka.WebApi.FileSystemTests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using FileSystem.FlowJS;
    using FluentAssertions;
    using Xunit;

    public class FlowParameterReaderFacts
    {
        [Fact]
        public void ReadParametersFromGetRequest()
        {
            /* given */
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    string.Format(
                        "https://files/upload?flowChunkNumber={0}&flowChunkSize={1}&flowTotalSize={2}&flowIdentifier={3}&flowFilename={4}",
                        1,
                        2048,
                        1024,
                        "0001-my-file.file",
                        "my-file.file"), UriKind.Absolute)
            };

            var reader = new FlowParametersReader();

            /* when */
            var parameters = reader.Read(httpRequest);

            /* then */
            parameters.FlowChunkNumber.ShouldBeEquivalentTo(1);
            parameters.FlowChunkSize.ShouldBeEquivalentTo(2048);
            parameters.FlowTotalSize.ShouldBeEquivalentTo(1024);
            parameters.FlowIdentifier.ShouldBeEquivalentTo("0001-my-file.file");
            parameters.FlowFilename.ShouldBeEquivalentTo("my-file.file");
        }

        [Fact]
        public void ReadParametersFromPostRequest()
        {
            /* given */
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://files/upload")
            };

            var formDataContent = new MultipartFormDataContent
            {
                {new StringContent("1"), "flowChunkNumber"},
                {new StringContent("2048"), "flowChunkSize"},
                {new StringContent("1024"), "flowTotalSize"},
                {new StringContent("0001-my-file.file"), "flowIdentifier"},
                {new StringContent("my-file.file"), "flowFilename"},
                {new StreamContent(new MemoryStream()), "content", "my-file.txt"}
            };


            var reader = new FlowParametersReader();

            /* when */
            var parameters = reader.Read(httpRequest);

            /* then */
            parameters.FlowChunkNumber.ShouldBeEquivalentTo(1);
            parameters.FlowChunkSize.ShouldBeEquivalentTo(2048);
            parameters.FlowTotalSize.ShouldBeEquivalentTo(1024);
            parameters.FlowIdentifier.ShouldBeEquivalentTo("0001-my-file.file");
            parameters.FlowFilename.ShouldBeEquivalentTo("my-file.file");
        }
    }
}