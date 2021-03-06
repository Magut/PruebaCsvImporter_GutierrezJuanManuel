using Azure.Storage.Blobs;
using CsvImporter.Controllers.Processors;
using CsvImporter.Controllers.Readers;
using CsvImporter.Controllers.Writers;
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
        /// Creates <see cref="IDataReader"/> instance
        /// </summary>
        /// <param name="streamReader">Stream from where to read</param>
        /// <param name="dataReadQueue">For enqueuing the read data</param>
        /// <param name="finishedReading">Indicates that the reader finished reading</param>
        /// <returns><see cref="IDataReader"/> instance</returns>
        public static IDataReader DataReaderCreator(StreamReader streamReader, ConcurrentQueue<string> dataReadQueue, Flag finishedReading)
        {
            return new DataReader(streamReader, dataReadQueue, finishedReading);
        }

        /// <summary>
        /// Creates <see cref="IFileReader"/> instance
        /// </summary>
        /// <param name="localFilePath">Path of the local file to be read</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <param name="finishedReading">Flag to indicate to the caller that it finished reading the file</param>
        /// <returns><see cref="IFileReader"/> instance</returns>
        public static IFileReader LocalFileReaderCreator(string localFilePath, ConcurrentQueue<string> queue, Flag finishedReading)
        {
            return new LocalFileReader(localFilePath, queue, finishedReading);
        }

        /// <summary>
        /// Creates <see cref="IFileReader"/> instance
        /// </summary>
        /// <param name="fileUrl">Url of the Azure file to be read</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <param name="finishedReading">Flag to indicate to the caller that it finished reading the file</param>
        /// <returns><see cref="IFileReader"/> instance</returns>
        public static IFileReader AzureFileReaderCreator(Uri fileUrl, ConcurrentQueue<string> queue, Flag finishedReading)
        {
            return new AzureFileReader(fileUrl, queue, finishedReading);
        }

        /// <summary>
        /// Creates <see cref="IFileReader"/> instance
        /// </summary>
        /// <param name="localFilePath">Path of the local file to be read (when the file is local)</param>
        /// <param name="fileUrl">Url of the Azure file to be read (when it reads from Azure)</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <param name="finishedReading">Flag to indicate to the caller that it finished reading the file</param>
        /// <returns><see cref="IFileReader"/> instance</returns>
        public static IFileReader FileReaderCreator(InputFileLocation inputFileLocation, string localFilePath, Uri fileUrl, ConcurrentQueue<string> queue, Flag finishedReading)
        {
            switch (inputFileLocation)
            {
                case InputFileLocation.Localhost:
                    return Factory.LocalFileReaderCreator(localFilePath, queue, finishedReading);
                case InputFileLocation.Azure:
                    return Factory.AzureFileReaderCreator(fileUrl, queue, finishedReading);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates <see cref="ICsvDataProcessor"/> instance
        /// </summary>
        /// <param name="dataReadQueue">Queue with data read (to be processed)</param>
        /// <param name="dataToWriteQueue">For enqueuing the processed data</param>
        /// <param name="readerFinishedReading">Indicates that the reader finished reading</param>
        /// <param name="processorFinishedProcessing">Indicates that this instance finished processing</param>
        /// <returns><see cref="ICsvDataProcessor"/> instance</returns>
        public static ICsvDataProcessor CsvDataProcessorCreator(ConcurrentQueue<string> dataReadQueue,
                                                                ConcurrentQueue<SingleDayStock> dataToWriteQueue,
                                                                Flag readerFinishedReading,
                                                                Flag processorFinishedProcessing)
        {
            return new CsvDataProcessor(dataReadQueue, dataToWriteQueue, readerFinishedReading, processorFinishedProcessing);
        }

        /// <summary>
        /// Creates <see cref="IStockWriter"/> instance
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="dataToWriteQueue">For dequeuing the data to be written</param>
        /// <param name="processorFinishedProcessing">Indicates that the processor finished processing</param>
        /// <param name="writerFinishedWriting">Indicates that this instance finished writing</param>
        /// <returns><see cref="IStockWriter"/> instance</returns>
        public static IStockWriter StockWriterCreator(string connectionString,
                                                      ConcurrentQueue<SingleDayStock> dataToWriteQueue,
                                                      Flag processorFinishedProcessing,
                                                      Flag writerFinishedWriting)
        {
            return new ImportedStockWriter(connectionString, dataToWriteQueue, processorFinishedProcessing, writerFinishedWriting);
        }
    }
}
