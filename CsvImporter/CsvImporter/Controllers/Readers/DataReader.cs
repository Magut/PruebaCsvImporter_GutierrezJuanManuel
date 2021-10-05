using CsvImporter.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace CsvImporter.Controllers.Readers
{
    /// <summary>
    /// Reads all the lines from a <see cref="StreamReader"/> and enqueues the rows
    /// in a <see cref="ConcurrentQueue{string}"/>
    /// </summary>
    public class DataReader : IDataReader
    {
        #region Constants for the operation

        private const int MaxQueueItems = 1000;
        private const int MilisecondsBetweenQueueSpaceChecks = 5;

        #endregion

        #region Initialization Fields

        private readonly StreamReader _stream;
        private readonly ConcurrentQueue<string> _queue;
        private readonly Flag _finishedReading;

        #endregion

        /// <summary>
        /// Constructor that takes a <see cref="StreamReader"/> from which it reads,
        /// and a <see cref="ConcurrentQueue{String}"/> to enqueue the data read
        /// </summary>
        /// <param name="stream"><see cref="StreamReader"/> to read from a file</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        public DataReader(StreamReader stream, ConcurrentQueue<string> queue)
        {
            ValidateInitialization(stream, queue);

            _stream = stream;
            _queue = queue;
        }

        /// <summary>
        /// Constructor that takes a <see cref="StreamReader"/> from which it reads,
        /// a <see cref="ConcurrentQueue{String}"/> to enqueue the data read,
        /// and a Flag to inform that it finished reading
        /// </summary>
        /// <param name="stream"><see cref="StreamReader"/> to read from a file</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <param name="finishedReading">Flag to indicate to the caller that it finished reading the file</param>
        public DataReader(StreamReader stream, ConcurrentQueue<string> queue, Flag finishedReading)
        {
            ValidateInitialization(stream, queue);

            _stream = stream;
            _queue = queue;

            if (finishedReading != null)
            {
                _finishedReading = finishedReading;
                _finishedReading.Event = false;
            }
        }

        /// <summary>
        /// It starts reading the stream until the end
        /// and equeues the rows read in the <see cref="ConcurrentQueue{String}"/> setted
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task StartReadingAndEqueuingDataAsync()
        {
            using (_stream)
            {
                // First row in stream file
                string row = await ReadNextRowAsync();

                if (row is null)
                {
                    Console.WriteLine("ERROR! The Stream has no rows to read.");
                    return;
                }

                if (!IsCsvHeader(row))
                {
                    _queue.Enqueue(row);
                }

                while ((row = await ReadNextRowAsync()) != null)
                {
                    _queue.Enqueue(row);
                    await WaitUntilQueueHasSpaceAsync();
                }

                if (_finishedReading != null)
                {
                    _finishedReading.Event = true;
                }
            }
        }

        /// <summary>
        /// It reads the next CSV row in the <see cref="StreamReader"/> of the class
        /// </summary>
        /// <remarks>
        /// <para>It doesn't has a "using" for the StreamReader</para>
        /// <para>The caller is responsible for handling the opening, closing and dispose of the stream</para>
        /// </remarks>
        /// <returns><see cref="String"/> with the CSV row</returns>
        private async Task<string> ReadNextRowAsync()
        {
            string row;

            try
            {
                row = await _stream.ReadLineAsync();
            }
            catch (ArgumentOutOfRangeException argEx)
            {
                Console.WriteLine($"ERROR! The number of characters in the next line is larger than MaxValue. Exception message: {0}", argEx.Message);
                return null;
            }
            catch (ObjectDisposedException objDisEx)
            {
                Console.WriteLine($"ERROR! The stream has been disposed. Exception message: {0}", objDisEx.Message);
                return null;
            }
            catch (InvalidOperationException invOpEx)
            {
                Console.WriteLine($"ERROR! The reader is currently in use by a previous read operation. Exception message: {0}", invOpEx.Message);
                return null;
            }

            return row;
        }

        /// <summary>
        /// It waits unit the queue has space to enqueue the new row
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        private async Task WaitUntilQueueHasSpaceAsync()
        {
            while(_queue.Count >= MaxQueueItems)
            {
                // TODO Esto podría ser un evento también disparado por quien desencola
                await Task.Delay(MilisecondsBetweenQueueSpaceChecks);
            }
        }

        /// <summary>
        /// Determines if the parameter <paramref name="row"/> is the Header of the CSV file
        /// </summary>
        /// <param name="row">Line from the CSV file</param>
        /// <returns><see langword="true"/> if it's the header, <see langword="false"/> else</returns>
        private static bool IsCsvHeader(string row)
        {
            return row.Contains("PointOfSale") || row.Contains("Product") || row.Contains("Date") || row.Contains("Stock");
        }

        /// <summary>
        /// It validates the parameters received in the constructor
        /// </summary>
        /// <remarks>Throws exception when a parameter is not valid</remarks>
        /// <param name="stream"><see cref="StreamReader"/> to read from a file</param>
        /// <param name="queue"><see cref="ConcurrentQueue{String}"/> to enqueue the data for the processor</param>
        /// <exception cref="ArgumentNullException">When a null argument is passed for a mandatory argument</exception>
        private void ValidateInitialization(StreamReader stream, ConcurrentQueue<string> queue)
        {
            if (stream is null || queue is null)
                throw new ArgumentNullException(stream is null ? nameof(stream) : nameof(queue), "ERROR! Cannot initialize with no valid stream and queue parameters.");
        }
    }
}
