using CsvImporter.Models;
using System;
using System.Data;
using Dapper;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace CsvImporter.Controllers.Writers
{
    /// <summary>
    /// Writes all the data enqueued in a <see cref="ConcurrentQueue{string}"/>
    /// </summary>
    public class ImportedStockWriter : IStockWriter
    {
        #region Constants for the operation

        private const string Query_StockHistory_InsertSingleRow = "INSERT INTO StockHistory (PointOfSale, Product, Date, Stock) Values (@PointOfSale, @Product, @Date, @Stock);";
        private const string Query_StockHistory_DeleteAllRows = "DELETE StockHistory;";
        private const string StoredProcedure_StockHistory_InsertCompressedData = "dbo.StockHistory_InsertCompressedData";

        private const int MiliSecondsBetweenDequeTries = 5;

        #endregion

        #region Initialization Fields

        private readonly string _connectionStringName;
        private readonly ConcurrentQueue<SingleDayStock> _dataProcessedQueue;
        private readonly Flag _processorFinishedProcessing;
        private readonly Flag _finishedWriting;

        #endregion

        /// <summary>
        /// Default constructor with ConnectionString name
        /// </summary>
        /// <param name="connectionStringName">ConnectionString name</param>
        /// <param name="dataProcessedQueue">Queue with the processed data</param>
        /// <param name="processorFinishedProcessing">Flag indicating that the processor finished enqueuing data</param>
        /// <param name="finishedWriting">Flag indicating that the writer finished writing data</param>
        public ImportedStockWriter(string connectionStringName,
                                   ConcurrentQueue<SingleDayStock> dataProcessedQueue,
                                   Flag processorFinishedProcessing,
                                   Flag finishedWriting)
        {
            ValidateInitialization(connectionStringName, dataProcessedQueue, processorFinishedProcessing, finishedWriting);

            _connectionStringName = connectionStringName;
            _dataProcessedQueue = dataProcessedQueue;
            _processorFinishedProcessing = processorFinishedProcessing;
            _finishedWriting = finishedWriting;
        }

        /// <summary>
        /// It starts writing in the target the data located in the input queue
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task StartWritingDataAsync()
        {
            while (!(_processorFinishedProcessing.Event && _dataProcessedQueue.IsEmpty))
            {
                // Checks if there is items in the input queue
                if (_dataProcessedQueue.IsEmpty)
                {
                    await Task.Delay(MiliSecondsBetweenDequeTries);
                    continue;
                }

                SingleDayStock singleDayStock = await GetItemFromReaderQueueAsync();
                if (singleDayStock is not null)
                    await InsertAsync(singleDayStock);
            }

            _finishedWriting.Event = true;
        }

        /// <summary>
        /// Inserts asynchrounously row/s to the StockHistory table
        /// </summary>
        /// <param name="data">Row/s with the data</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task InsertAsync(IDayStock data)
        {
            using (IDbConnection connection = DatabaseHelper.CreateDbConnection(_connectionStringName))
            {
                try
                {
                    if (data is SingleDayStock d)
                        await InsertDataAsync(connection, d);
                    if (data is MultipleDaysStock cD)
                        await InsertDataAsync(connection, cD);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR! InsertAsync -> An Exception was thrown with the message: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Deletes all stock rows asynchrounously in the target
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task DeleteAllAsync()
        {
            using (IDbConnection connection = DatabaseHelper.CreateDbConnection(_connectionStringName))
            {
                try
                {
                    int affectedRows = await connection.ExecuteAsync(Query_StockHistory_DeleteAllRows);
                    Debug.WriteLine($"DeleteAllAsync -> AffectedRows: {affectedRows}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR! DeleteAllAsync -> An Exception was thrown with the message: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Inserts one row to StockHistory table
        /// </summary>
        /// <param name="connection"><see cref="IDbConnection"/>that holds the session to do the operation</param>
        /// <param name="data"><see cref="SingleDayStock"/>Stock data</param>
        private static async Task InsertDataAsync(IDbConnection connection, SingleDayStock data)
        {
            int affectedRows = await connection.ExecuteAsync(Query_StockHistory_InsertSingleRow, data);
            Debug.WriteLine($"InsertDataAsync<SingleDayStock> -> AffectedRows: {affectedRows}");
        }

        /// <summary>
        /// Inserts one or more rows to StockHistory table
        /// </summary>
        /// <param name="connection"><see cref="IDbConnection"/>that holds the session to do the operation</param>
        /// <param name="compressedData"><see cref="CompressedData"/>Stock data in a range of days</param>
        private static async Task InsertDataAsync(IDbConnection connection, MultipleDaysStock compressedData)
        {
            int affectedRows = await connection.ExecuteAsync(StoredProcedure_StockHistory_InsertCompressedData, compressedData);
            Debug.WriteLine($"InsertDataAsync<MultipleDayStock> -> AffectedRows: {affectedRows}");
        }

        /// <summary>
        /// Tries until it get a valid stock from the processor queue
        /// </summary>
        /// <returns><see cref="SingleDayStock"/> with the data</returns>
        private async Task<SingleDayStock> GetItemFromReaderQueueAsync()
        {
            SingleDayStock stock;

            while (!_dataProcessedQueue.TryDequeue(out stock))
            {
                Debug.WriteLine($"No pude desencolar, intentaré nuevamente luego de {MiliSecondsBetweenDequeTries} mseg.");
                await Task.Delay(MiliSecondsBetweenDequeTries);
                return null;
            }

            return stock;
        }

        /// <summary>
        /// It validates the parameters received in the constructor
        /// </summary>
        /// <remarks>Throws exception when a parameter is not valid</remarks>
        /// <param name="connectionStringName">ConnectionString name</param>
        /// <param name="dataProcessedQueue">Queue with the processed data</param>
        /// <param name="processorFinishedProcessing">Flag indicating that the processor finished enqueuing data</param>
        /// <param name="finishedWriting">Flag indicating that the writer finished writing data</param>
        /// <exception cref="ArgumentNullException">When a null argument is passed for a mandatory argument</exception>
        private void ValidateInitialization(string connectionStringName,
                                            ConcurrentQueue<SingleDayStock> dataProcessedQueue,
                                            Flag processorFinishedProcessing,
                                            Flag finishedWriting)
        {
            if (connectionStringName is null || String.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentNullException(nameof(connectionStringName), "ERROR! Cannot initialize without connection string.");

            if (dataProcessedQueue is null)
                throw new ArgumentNullException(nameof(dataProcessedQueue), "ERROR! Cannot initialize with invalid queue parameter.");

            if (processorFinishedProcessing is null || finishedWriting is null)
                throw new ArgumentNullException(processorFinishedProcessing is null ? nameof(processorFinishedProcessing) : nameof(finishedWriting), $"ERROR! Cannot initialize with invalid flag parameter.");
        }
    }
}
