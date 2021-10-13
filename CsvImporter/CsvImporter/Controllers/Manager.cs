using CsvImporter.Controllers.Readers;
using CsvImporter.Controllers.Writers;
using CsvImporter.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CsvImporter.Controllers
{
    public class Manager
    {
        #region Constants for the operation

        private const int NumberOfProcessorTasksDefault = 30;
        private const int NumberOfWriterTasksDefault = 30;

        #endregion

        #region Settings

        private int _numberOfProcessorTasks = NumberOfProcessorTasksDefault;
        private int _numberOfWriterTasks = NumberOfWriterTasksDefault;

        public int NumberOfProcessorTasks
        {
            get => _numberOfProcessorTasks;
            set => _numberOfProcessorTasks = value;
        }

        public int NumberOfWriterTasks
        {
            get => _numberOfWriterTasks;
            set => _numberOfWriterTasks = value;
        }

        #endregion

        private readonly InputFileLocation _inputFileLocation;
        private readonly string _filePath;
        private readonly Uri _fileUrl;

        private ConcurrentQueue<string> _dataReadQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<SingleDayStock> _dataToWriteQueue = new ConcurrentQueue<SingleDayStock>();
        private Flag _readerFinishedReading = new Flag();
        private Flag _processorFinishedProcessing = new Flag();
        private Flag _writerFinishedWriting = new Flag();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputFileLocation">Indicates where the file is located</param>
        /// <param name="filePath">(Local or remote) Path/URL of the file</param>
        public Manager(InputFileLocation inputFileLocation, string filePath)
        {
            _inputFileLocation = inputFileLocation;
            _filePath = filePath;

            ValidateInitialization(_inputFileLocation, ref _filePath, out _fileUrl);
        }

        /// <summary>
        /// Import the data from the source passed in the arguments to the console app
        /// into the database
        /// </summary>
        public void Import()
        {
            Console.WriteLine("Importing data from CSV file into the Database...");
            List<Task> allTasks = new List<Task>();


            // Itinialize the Reader
            IFileReader reader = Factory.FileReaderCreator(_inputFileLocation, _filePath, _fileUrl, _dataReadQueue, _readerFinishedReading);
            Task readerTask = Task.Run(async () => await reader.ReadFileAndEnqueueDataAsync())
                                  .ContinueWith(t => { Console.WriteLine($"Reader Task Faulted! Exception type: {t.Exception.InnerException.GetType().Name} - Message: {t.Exception.InnerException.Message}"); }, TaskContinuationOptions.OnlyOnFaulted);
            allTasks.Add(readerTask);


            // Initialize the Processors
            for (int i = 0; i < NumberOfProcessorTasks; i++)
            {
                Task newProcessor = Task.Run(async () => await Factory.CsvDataProcessorCreator(_dataReadQueue,
                                                                                               _dataToWriteQueue,
                                                                                               _readerFinishedReading,
                                                                                               _processorFinishedProcessing)
                                                                      .ProcessAndEnqueueDataAsync())
                                        .ContinueWith(t => { Console.WriteLine($"Process Task Faulted! Exception type: {t.Exception.InnerException.GetType().Name} - Message: {t.Exception.InnerException.Message}"); }, TaskContinuationOptions.OnlyOnFaulted);
                allTasks.Add(newProcessor);
            }


            // Initialize the Writer to Delete the previous data
            string connectionStringName = "Stock";
            IStockWriter writerDeletePreviousData = Factory.StockWriterCreator(connectionStringName, _dataToWriteQueue, _processorFinishedProcessing, _writerFinishedWriting);
            Task deleteTask = Task.Run(async () => { await writerDeletePreviousData.DeleteAllAsync(); });
            deleteTask.Wait();

            // Initialize the Writers to Write to database
            for (int i = 0; i < NumberOfWriterTasks; i++)
            {
                Task newWriter = Task.Run(async () => { await Factory.StockWriterCreator(connectionStringName,
                                                                                         _dataToWriteQueue,
                                                                                         _processorFinishedProcessing,
                                                                                         _writerFinishedWriting)
                                                                     .WriteImportedDataAsync(); })
                                     .ContinueWith(t => { Console.WriteLine($"Write Task Faulted! Exception type: {t.Exception.InnerException.GetType().Name} - Message: {t.Exception.InnerException.Message}"); }, TaskContinuationOptions.OnlyOnFaulted);
                allTasks.Add(newWriter);
            }

            try
            {
                Task.WaitAll(allTasks.ToArray());
            }
            catch (AggregateException ae)
            {
                foreach(Exception e in ae.InnerExceptions)
                {
                    Console.WriteLine($"Task exception. Type: {e.GetType().Name} - Message: {e.Message}");
                }
            }

            Console.WriteLine("Finished writing data.");
            Console.ReadKey();
        }

        /// <summary>
        /// It validates the parameters received in the constructor
        /// </summary>
        /// <remarks>Throws exception when a parameter is not valid</remarks>
        /// <param name="inputFileLocation">Indicating whether it is a local or remote file and in which server is located</param>
        /// <param name="filePath">Path of the local file to be read or URL of the remote file</param>
        /// <param name="url">Parsed URL</param>
        /// <exception cref="ArgumentException">When an invalid argument is passed for a mandatory argument</exception>
        private void ValidateInitialization(InputFileLocation inputFileLocation, ref string filePath, out Uri url)
        {
            url = null;
            bool validPath = inputFileLocation == InputFileLocation.Localhost ? CheckValidLocalPath(ref filePath) : CheckValidUrl(filePath, out url);

            if (!validPath)
            {
                throw new ArgumentException($"The {(inputFileLocation == InputFileLocation.Localhost ? "local file path" : "URL")} parameter is not valid", nameof(filePath));
            }
        }

        /// <summary>
        /// Checks if the parameter <paramref name="urlString"/> looks like a valid URL
        /// </summary>
        /// <param name="urlString">URL</param>
        /// <param name="url">Parsed URL</param>
        /// <returns><see langword="true"/> if the <paramref name="urlString"/> looks like a valid URL, else <see langword="false"/></returns>
        private static bool CheckValidUrl(string urlString, out Uri url)
        {
            return Uri.TryCreate(urlString, UriKind.Absolute, out url) && (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Checks if the parameter <paramref name="path"/> looks like a valid Path
        /// </summary>
        /// <param name="path">Path (returned as a full local path)</param>
        /// <returns><see langword="true"/> if the <paramref name="path"/> looks like a valid Path, else <see langword="false"/></returns>
        private static bool CheckValidLocalPath(ref string path)
        {
            string fullPath;

            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                Console.WriteLine("Please enter a valid file path.");
                return false;
            }

            path = fullPath;
            return true;
        }
    }
}
