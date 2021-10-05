using CsvImporter.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace CsvImporter.Controllers.Readers
{
    /// <summary>
    /// Reads all the lines from a local file and enqueues the rows
    /// in a <see cref="ConcurrentQueue{string}"/>
    /// </summary>
    public class LocalFileReader : IFileReader
    {
        #region Initialization Fields

        private readonly string _localFilePath;
        private readonly ConcurrentQueue<string> _queue;
        private readonly Flag _finishedReading;

        #endregion

        /// <summary>
        /// Constructor that takes a <see cref="ConcurrentQueue{String}"/> to enqueue the data read
        /// </summary>
        /// <param name="localFilePath">Path of the local file to be read</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        public LocalFileReader(string localFilePath, ConcurrentQueue<string> queue)
        {
            ValidateInitialization(localFilePath, queue);

            _localFilePath = localFilePath;
            _queue = queue;

            _finishedReading = new Flag();
            _finishedReading.Event = false;
        }

        /// <summary>
        /// Constructor that takes a <see cref="ConcurrentQueue{String}"/> to enqueue the data read
        /// </summary>
        /// <param name="localFilePath">Path of the local file to be read</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <param name="finishedReading">Flag to indicate to the caller that it finished reading the file</param>
        public LocalFileReader(string localFilePath, ConcurrentQueue<string> queue, Flag finishedReading)
        {
            ValidateInitialization(localFilePath, queue);

            _localFilePath = localFilePath;
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
            using (StreamReader streamReader = new StreamReader(_localFilePath))
            {
                DataReader reader = new DataReader(streamReader, _queue, _finishedReading);
                await reader.StartReadingAndEqueuingDataAsync();
            }
        }

        /// <summary>
        /// It validates the parameters received in the constructor
        /// </summary>
        /// <remarks>Throws exception when a parameter is not valid</remarks>
        /// <param name="localFilePath">Path of the local file to be read</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <exception cref="ArgumentNullException">When a null argument is passed for a mandatory argument</exception>
        /// <exception cref="ArgumentException">When an invalid argument is passed for a mandatory argument</exception>
        private void ValidateInitialization(string localFilePath, ConcurrentQueue<string> queue)
        {
            if (localFilePath is null || queue is null)
                throw new ArgumentNullException(localFilePath is null ? "localFilePath" : "queue", "ERROR! Cannot initialize with invalid parameters.");

            if (String.IsNullOrWhiteSpace(localFilePath))
                throw new ArgumentException("ERROR! Cannot initialize with invalid parameters.", "localFilePath");
        }
    }
}
