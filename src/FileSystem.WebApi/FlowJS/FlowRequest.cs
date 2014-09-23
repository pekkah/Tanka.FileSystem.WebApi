namespace Tanka.FileSystem.WebApi.FlowJS
{
    using System;

    public class FlowRequest
    {
        /// <summary>
        /// The index of the chunk in the current upload. 
        /// First chunk is 1 (no base-0 counting here).
        /// </summary>
        public ulong? FlowChunkNumber { get; set; } 
        
        /// <summary>
        /// The total number of chunks.
        /// </summary>
        public ulong? FlowTotalChunks { get; set; }

        /// <summary>
        /// The general chunk size. Using this value and flowTotalSize 
        /// you can calculate the total number of chunks. Please note 
        /// that the size of the data received in the HTTP might be 
        /// lower than flowChunkSize of this for the last chunk for a
        /// file.
        /// </summary>
        public ulong? FlowChunkSize { get; set; }

        /// <summary>
        /// The total file size.
        /// </summary>
        public ulong? FlowTotalSize { get; set; }

        /// <summary>
        /// A unique identifier for the file contained in the request.
        /// </summary>
        public string FlowIdentifier { get; set; }

        /// <summary>
        /// The original file name (since a bug in Firefox results in 
        /// the file name not being transmitted in chunk multipart posts).
        /// </summary>
        public string FlowFilename { get; set; }

        /// <summary>
        /// The file's relative path when selecting a directory 
        /// (defaults to file name in all browsers except Chrome).
        /// </summary>
        public string FlowRelativePath { get; set; }

        public bool IsLastChunk
        {
            get { return FlowChunkNumber == FlowTotalChunks; }
        }

        public Tuple<string, string> TemporaryFile { get; set; }
    }
}