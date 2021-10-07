using Azure.Storage.Blobs;
using CsvImporter.Controllers.Readers;
using CsvImporter.Models;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace CsvImporter.Controllers
{
    /// <summary>
    /// Factory class for creating instances for dependency injection
    /// </summary>
    class Factory
    {
        /// <summary>
        /// Creates <see cref="StreamReader"/> instance
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns><see cref="StreamReader"/> instance</returns>
        public static StreamReader StreamReaderCreator(string path)
        {
            return new StreamReader(path);
        }

        /// <summary>
        /// Creates <see cref="StreamReader"/> instance
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <returns><see cref="StreamReader"/> instance</returns>
        public static StreamReader StreamReaderCreator(Stream stream)
        {
            return new StreamReader(stream);
        }

        /// <summary>
        /// Creates <see cref="BlobClient"/> instance
        /// </summary>
        /// <param name="fileUrl">File URL</param>
        /// <returns><see cref="BlobClient"/> instance</returns>
        public static BlobClient BlobClientCreator(Uri fileUrl)
        {
            return new BlobClient(fileUrl);
        }

        /// <summary>
        /// Creates <see cref="BlobClient"/> instance
        /// </summary>
        /// <param name="streamReader">Stream from where to read</param>
        /// <param name="dataReadQueue">For enqueuing the read data</param>
        /// <param name="finishedReading">Indicates that the reader finished reading</param>
        /// <returns><see cref="BlobClient"/> instance</returns>
        public static DataReader DataReaderCreator(StreamReader streamReader, ConcurrentQueue<string> dataReadQueue, Flag finishedReading)
        {
            return new DataReader(streamReader, dataReadQueue, finishedReading);
        }
    }
}
