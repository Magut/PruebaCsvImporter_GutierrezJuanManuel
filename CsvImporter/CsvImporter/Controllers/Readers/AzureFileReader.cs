using Azure.Storage.Blobs;
using CsvImporter.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace CsvImporter.Controllers.Readers
{
    /// <summary>
    /// Reads all the lines from an Azure blob file and enqueues the rows
    /// in a <see cref="ConcurrentQueue{string}"/>
    /// </summary>
    public class AzureFileReader : IFileReader
    {
        #region Initialization Fields

        private readonly Uri _fileUrl;
        private readonly ConcurrentQueue<string> _queue;
        private readonly Flag _finishedReading;

        #endregion

        /// <summary>
        /// Constructor that takes a <see cref="ConcurrentQueue{String}"/> to enqueue the data read
        /// </summary>
        /// <param name="fileUrl">Url of the Azure file to be read</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        public AzureFileReader(Uri fileUrl, ConcurrentQueue<string> queue)
        {
            ValidateInitialization(fileUrl, queue);

            _fileUrl = fileUrl;
            _queue = queue;

            _finishedReading = new Flag();
            _finishedReading.Event = false;
        }

        /// <summary>
        /// Constructor that takes a <see cref="ConcurrentQueue{String}"/> to enqueue the data read
        /// </summary>
        /// <param name="fileUrl">Url of the Azure file to be read</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <param name="finishedReading">Flag to indicate to the caller that it finished reading the file</param>
        public AzureFileReader(Uri fileUrl, ConcurrentQueue<string> queue, Flag finishedReading)
        {
            ValidateInitialization(fileUrl, queue);

            _fileUrl = fileUrl;
            _queue = queue;

            if (finishedReading != null)
            {
                _finishedReading = finishedReading;
            }
            else
            {
                _finishedReading = new Flag();
            }

            _finishedReading.Event = false;
        }

        /// <summary>
        /// It starts reading the file until the end
        /// and equeues the rows read in a <see cref="ConcurrentQueue{String}"/>
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task ReadFileAndEnqueueDataAsync()
        {
            using (Stream stream = await new BlobClient(_fileUrl).OpenReadAsync())
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    DataReader reader = new DataReader(streamReader, _queue, _finishedReading);
                    await reader.ReadAndEnqueueDataAsync();
                }
            }
        }

        /// <summary>
        /// It validates the parameters received in the constructor
        /// </summary>
        /// <remarks>Throws exception when a parameter is not valid</remarks>
        /// <param name="fileUrl">Url of the Azure file to be read</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <exception cref="ArgumentNullException">When a null argument is passed for a mandatory argument</exception>
        private void ValidateInitialization(Uri fileUrl, ConcurrentQueue<string> queue)
        {
            if (fileUrl is null || queue is null)
                throw new ArgumentNullException(fileUrl is null ? nameof(fileUrl) : nameof(queue), "ERROR! Cannot initialize with invalid parameters.");
        }
    }
}
