using System.Threading;
using System.Threading.Tasks;

namespace CsvImporter.Controllers.Readers
{
    /// <summary>
    /// Defines needed methods for a data reader
    /// </summary>
    interface IDataReader
    {
        /// <summary>
        /// It starts reading until the end
        /// and equeues the rows read in a queue
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public Task ReadAndEnqueueDataAsync();
        /// <summary>
        /// It starts reading until the end
        /// and equeues the rows read in a queue
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public Task ReadAndEnqueueDataAsync(CancellationToken cancellationToken);
    }
}
