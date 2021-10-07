﻿using CsvImporter.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CsvImporter.Controllers.Processors
{
    /// <summary>
    /// Class for instances of processors who read CSV data from a queue,
    /// parse it to a valid class objects, and enqueue it for the writer
    /// </summary>
    public class CsvDataProcessor : ICsvDataProcessor
    {
        # region Constants for the operation
        
        private const int MiliSecondsBetweenDequeTries = 5;
        private const int MiliSecondsBetweenQueueSpaceChecks = 5;
        private const int MaxDataToWriteQueueItems = 1000;

        #endregion

        #region Initialization Fields

        private readonly ConcurrentQueue<string> _readerQueue;
        private readonly ConcurrentQueue<SingleDayStock> _dataToWriteQueue;
        private readonly Flag _readerFinishedReading;
        private readonly Flag _finishedEqueuingDataForWriter;

        #endregion

        /// <summary>
        /// Constructuro with the needed queues and flags
        /// </summary>
        /// <param name="readerQueue">Queue where the reader enqueues the data</param>
        /// <param name="dataToWriteQueue">Queue in which the parsed data is enqueued for the writer</param>
        /// <param name="readerFinishedReading">Flag indicating that the reader finished reading
        /// and is not going to continue enqueuing</param>
        /// <param name="finishedEnqueuingDataForWriter">Flag indicating that this processor finished enqueuing
        /// data for the writer</param>
        public CsvDataProcessor(ConcurrentQueue<string> readerQueue,
                                ConcurrentQueue<SingleDayStock> dataToWriteQueue,
                                Flag readerFinishedReading,
                                Flag finishedEnqueuingDataForWriter)
        {
            ValidateInitialization(readerQueue, dataToWriteQueue, readerFinishedReading, finishedEnqueuingDataForWriter);

            _readerQueue = readerQueue;
            _dataToWriteQueue = dataToWriteQueue;
            _readerFinishedReading = readerFinishedReading;
            _finishedEqueuingDataForWriter = finishedEnqueuingDataForWriter;
            _finishedEqueuingDataForWriter.Event = false;
        }

        /// <summary>
        /// It starts processing the CSV rows enqueued by the reader and enqueues
        /// the data read in a <see cref="ConcurrentQueue{SingleDayStock}"/> to feed the writer
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task ProcessAndEnqueueDataAsync()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await ProcessAndEnqueueDataAsync(cancellationToken);
        }

        /// <summary>
        /// It starts processing the CSV rows enqueued by the reader and enqueues
        /// the data read in a <see cref="ConcurrentQueue{SingleDayStock}"/> to feed the writer
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task ProcessAndEnqueueDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !(_readerFinishedReading.Event && _readerQueue.IsEmpty))
            {
                // Checks if there is space in the output queue
                if (_dataToWriteQueue.Count > MaxDataToWriteQueueItems)
                {
                    Debug.WriteLine("Se llenó la cola de datos procesados y aún no insertados en BD. Espero y vuelvo a chequeuar.");
                    await Task.Delay(MiliSecondsBetweenQueueSpaceChecks, cancellationToken);
                    continue;
                }

                // Checks if there is items in the input queue
                if (_readerQueue.IsEmpty)
                {
                    await Task.Delay(MiliSecondsBetweenDequeTries, cancellationToken);
                    continue;
                }

                SingleDayStock singleDayStock = await GetItemFromReaderQueueAsync(cancellationToken);
                if (singleDayStock is not null)
                    _dataToWriteQueue.Enqueue(singleDayStock);
            }
            
            _finishedEqueuingDataForWriter.Event = true;
        }

        /// <summary>
        /// Tries until it get a valid CSV row from the reader queue,
        /// and returns it parsed to SingleDayStock
        /// </summary>
        /// <returns><see cref="SingleDayStock"/> with the row data parsed</returns>
        private async Task<SingleDayStock> GetItemFromReaderQueueAsync(CancellationToken cancellationToken)
        {
            string csvRow = string.Empty;

            while(!cancellationToken.IsCancellationRequested && !_readerQueue.TryDequeue(out csvRow))
            {
                Debug.WriteLine($"No pude desencolar, intentaré nuevamente luego de {MiliSecondsBetweenDequeTries} mseg.");
                await Task.Delay(MiliSecondsBetweenDequeTries, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
                return null;
            else
                return new SingleDayStock(csvRow);
        }

        /// <summary>
        /// It validates the parameters received in the constructor
        /// </summary>
        /// <remarks>Throws exception when a parameter is not valid</remarks>
        /// <param name="readerQueue">Queue where the reader enqueues the data</param>
        /// <param name="dataToWriteQueue">Queue in which the parsed data is enqueued for the writer</param>
        /// <param name="readerFinishedReading">Flag indicating that the reader finished reading
        /// and is not going to continue enqueuing</param>
        /// <param name="finishedEnqueuingDataForWriter">Flag indicating that this processor finished enqueuing
        /// data for the writer</param>
        /// <exception cref="ArgumentNullException">When a null argument is passed for a mandatory argument</exception>
        private void ValidateInitialization(ConcurrentQueue<string> readerQueue,
                                            ConcurrentQueue<SingleDayStock> dataToWriteQueue,
                                            Flag readerFinishedReading,
                                            Flag finishedEnqueuingDataForWriter)
        {
            if (readerQueue is null || dataToWriteQueue is null)
                throw new ArgumentNullException(readerQueue is null ? nameof(readerQueue) : nameof(dataToWriteQueue), "ERROR! Cannot initialize with invalid parameters.");

            if (readerFinishedReading is null || finishedEnqueuingDataForWriter is null)
                throw new ArgumentNullException(readerFinishedReading is null ? nameof(readerFinishedReading) : nameof(finishedEnqueuingDataForWriter), "ERROR! Cannot initialize with invalid parameters.");
        }
    }
}
