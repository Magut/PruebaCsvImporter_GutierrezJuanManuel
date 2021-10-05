using System.Threading.Tasks;

namespace CsvImporter.Controllers.Readers
{
    /// <summary>
    /// Defines needed methods for a file reader
    /// </summary>
    interface IFileReader
    {
        /// <summary>
        /// It starts reading the file until the end
        /// and equeues the rows read in a queue setted
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public Task ReadFileAndEnqueueDataAsync();
    }
}
