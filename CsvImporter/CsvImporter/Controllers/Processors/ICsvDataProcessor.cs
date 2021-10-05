using System.Threading.Tasks;

namespace CsvImporter.Controllers.Processors
{
    /// <summary>
    /// Defines needed methods for a CSV Data Processor
    /// </summary>
    interface ICsvDataProcessor
    {
        /// <summary>
        /// It starts processing the CSV rows enqueued by the reader
        /// and enqueues the data read in another queue to feed the writer
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public Task StartProcessingAndEnqueuingDataAsync();
    }
}
