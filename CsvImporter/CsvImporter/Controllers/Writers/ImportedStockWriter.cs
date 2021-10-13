using CsvImporter.Models;
using System;
using System.Data;
using Dapper;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Reflection;

namespace CsvImporter.Controllers.Writers
{
    /// <summary>
    /// Writes all the data enqueued in a <see cref="ConcurrentQueue{string}"/>
    /// </summary>
    public class ImportedStockWriter : IStockWriter
    {
        #region Constants for the operation

        private const string Query_StockHistory_InsertSingleRow = "INSERT INTO StockHistory ([PointOfSale], [Product], [Date], [Stock]) Values (@PointOfSale, @Product, @Date, @Stock);";
        private const string Query_StockHistory_DeleteAllRows = "DELETE StockHistory;";
        private const string Query_StockHistory_DeleteNRows = "DELETE TOP (@MaxRowsToDelete) FROM Stock.dbo.StockHistory;";
        private const string StoredProcedure_StockHistory_InsertCompressedData = "dbo.StockHistory_InsertCompressedData";

        private const int MaxRowsToDeleteInASingleExecutionDefault = 100000;
        private const int MiliSecondsBetweenDequeTriesDefault = 5;

        #endregion

        #region Initialization Fields

        private readonly string _connectionStringName;
        private readonly ConcurrentQueue<SingleDayStock> _dataProcessedQueue;
        private readonly Flag _processorFinishedProcessing;
        private readonly Flag _finishedWriting;

        #endregion

        #region Settings

        private int _maxRowsToDeleteInASingleExecution = MaxRowsToDeleteInASingleExecutionDefault;
        private int _miliSecondsBetweenDequeTries = MiliSecondsBetweenDequeTriesDefault;

        public int MaxRowsToDeleteInASingleExecution
        {
            get => _maxRowsToDeleteInASingleExecution;
            set => _maxRowsToDeleteInASingleExecution = value;
        }
        public int MiliSecondsBetweenDequeTries
        {
            get => _miliSecondsBetweenDequeTries;
            set => _miliSecondsBetweenDequeTries = value;
        }

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
        public async Task WriteImportedDataAsync()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await WriteImportedDataAsync(cancellationToken);
        }

        /// <summary>
        /// It starts writing in the target the data located in the input queue
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task WriteImportedDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && !(_processorFinishedProcessing.Event && _dataProcessedQueue.IsEmpty))
                {
                    // Checks if there is items in the input queue
                    if (_dataProcessedQueue.IsEmpty)
                    {
                        await Task.Delay(MiliSecondsBetweenDequeTries, cancellationToken);
                        continue;
                    }

                    SingleDayStock singleDayStock = await GetItemFromReaderQueueAsync(cancellationToken);
                    if (singleDayStock is not null)
                    {
                        await InsertAsync(singleDayStock, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException opCanceledEx)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name} => Operation was cancelled successfully. Message: {opCanceledEx.Message}");
            }
            catch (AggregateException ae)
            {
                foreach(var e in ae.InnerExceptions)
                {
                    Console.WriteLine($"Writer excpetion. Message: {e.Message}");
                }
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
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await InsertAsync(data, cancellationToken);
        }

        /// <summary>
        /// Inserts asynchrounously row/s to the StockHistory table
        /// </summary>
        /// <param name="data">Row/s with the data</param>
        /// <param name="cancellationToken">Token for cancellation</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task InsertAsync(IDayStock data, CancellationToken cancellationToken)
        {
            try
            {
                using (IDbConnection connection = DatabaseHelper.CreateDbConnection(_connectionStringName))
                {
                    try
                    {
                        if (data is SingleDayStock d)
                            await InsertDataAsync(connection, d, cancellationToken);
                        if (data is MultipleDaysStock cD)
                            await InsertDataAsync(connection, cD, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR! InsertAsync -> An Exception was thrown with the message: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException opCanceledEx)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name} => Operation was cancelled successfully. Message: {opCanceledEx.Message}");
            }
        }

        /// <summary>
        /// Deletes all stock rows asynchrounously in the target
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task DeleteAllAsync()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await DeleteAllAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes all stock rows asynchrounously in the target
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (IDbConnection connection = DatabaseHelper.CreateDbConnection(_connectionStringName))
                {
                    try
                    {
                        int affectedRows = 0;
                        do
                        {
                            //affectedRows = connection.Execute("DELETE Stock.dbo.StockHistory;");
                            //affectedRows = await connection.ExecuteAsync("DELETE Stock.dbo.StockHistory;");
                            affectedRows = connection.Execute(Query_StockHistory_DeleteNRows, new { MaxRowsToDelete = MaxRowsToDeleteInASingleExecution });
                            //affectedRows = await connection.ExecuteAsync(Query_StockHistory_DeleteNRows, new { MaxRowsToDelete = MaxRowsToDeleteInASingleExecution });
                            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name} -> AffectedRows: {affectedRows}");
                        } while (affectedRows > 0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR! {MethodBase.GetCurrentMethod().Name} -> An Exception was thrown with the message: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException opCanceledEx)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name} => Operation was cancelled successfully. Message: {opCanceledEx.Message}");
            }
        }

        /// <summary>
        /// Inserts one row to StockHistory table
        /// </summary>
        /// <param name="connection"><see cref="IDbConnection"/>that holds the session to do the operation</param>
        /// <param name="singleDayStock"><see cref="SingleDayStock"/>Stock data</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        private static async Task InsertDataAsync(IDbConnection connection, SingleDayStock singleDayStock)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await InsertDataAsync(connection, singleDayStock, cancellationToken);
        }

        /// <summary>
        /// Inserts one row to StockHistory table
        /// </summary>
        /// <param name="connection"><see cref="IDbConnection"/>that holds the session to do the operation</param>
        /// <param name="singleDayStock"><see cref="SingleDayStock"/>Stock data</param>
        /// <param name="cancellationToken">Token for cancellation</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        private static async Task InsertDataAsync(IDbConnection connection, SingleDayStock singleDayStock, CancellationToken cancellationToken)
        {
            try
            {
                //int affectedRows = await connection.ExecuteAsync(Query_StockHistory_InsertSingleRow, singleDayStock);
                int affectedRows = connection.Execute(Query_StockHistory_InsertSingleRow, singleDayStock);
                Debug.WriteLine($"InsertDataAsync<SingleDayStock> -> AffectedRows: {affectedRows}");
            }
            catch (OperationCanceledException opCanceledEx)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name} => Operation was cancelled successfully. Message: {opCanceledEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR! {MethodBase.GetCurrentMethod().Name} -> An Exception was thrown with the message: {ex.Message}");
            }
        }

        /// <summary>
        /// Inserts one or more rows to StockHistory table
        /// </summary>
        /// <param name="connection"><see cref="IDbConnection"/>that holds the session to do the operation</param>
        /// <param name="multipleDaysStock"><see cref="CompressedData"/>Stock data in a range of days</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        private static async Task InsertDataAsync(IDbConnection connection, MultipleDaysStock multipleDaysStock)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await InsertDataAsync(connection, multipleDaysStock, cancellationToken);
        }

        /// <summary>
        /// Inserts one or more rows to StockHistory table
        /// </summary>
        /// <param name="connection"><see cref="IDbConnection"/>that holds the session to do the operation</param>
        /// <param name="multipleDaysStock"><see cref="CompressedData"/>Stock data in a range of days</param>
        /// <param name="cancellationToken">Token for cancellation</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        private static async Task InsertDataAsync(IDbConnection connection, MultipleDaysStock multipleDaysStock, CancellationToken cancellationToken)
        {
            try
            {
                int affectedRows = await connection.ExecuteAsync(StoredProcedure_StockHistory_InsertCompressedData, multipleDaysStock);
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}<MultipleDayStock> -> AffectedRows: {affectedRows}");
            }
            catch (OperationCanceledException opCanceledEx)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name} => Operation was cancelled successfully. Message: {opCanceledEx.Message}");
            }
        }

        /// <summary>
        /// Tries until it get a valid stock from the processor queue
        /// </summary>
        /// <returns><see cref="SingleDayStock"/> with the data</returns>
        private async Task<SingleDayStock> GetItemFromReaderQueueAsync()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            return await GetItemFromReaderQueueAsync(cancellationToken);
        }

        /// <summary>
        /// Tries until it get a valid stock from the processor queue
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation</param>
        /// <returns><see cref="SingleDayStock"/> with the data</returns>
        private async Task<SingleDayStock> GetItemFromReaderQueueAsync(CancellationToken cancellationToken)
        {
            SingleDayStock stock = null;

            try
            {
                while (!cancellationToken.IsCancellationRequested && !_dataProcessedQueue.TryDequeue(out stock))
                {
                    Debug.WriteLine($"No pude desencolar, intentaré nuevamente luego de {MiliSecondsBetweenDequeTries} mseg.");
                    await Task.Delay(MiliSecondsBetweenDequeTries);
                    return null;
                }
            }
            catch (OperationCanceledException opCanceledEx)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name} => Operation was cancelled successfully. Message: {opCanceledEx.Message}");
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
