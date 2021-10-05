using CsvImporter.Models;
using System.Threading.Tasks;

namespace CsvImporter.Controllers.Writers
{
    /// <summary>
    /// Defines the methods for an Imported Stock Data Writer
    /// </summary>
    interface IStockWriter
    {
        /// <summary>
        /// It starts writing in the target the data located in the input queue
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public Task StartWritingDataAsync();
        /// <summary>
        /// Insert asynchrounously row/s in the target
        /// </summary>
        /// <param name="data">Row/s with the data</param>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public Task InsertAsync(IDayStock data);
        /// <summary>
        /// Deletes all stock rows asynchrounously in the target
        /// </summary>
        /// <returns>No object or value is returned by this method when it completes</returns>
        public Task DeleteAllAsync();
    }
}
