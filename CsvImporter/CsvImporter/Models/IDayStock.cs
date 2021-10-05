

namespace CsvImporter.Models
{
    /// <summary>
    /// Defines the methods for a day stock
    /// </summary>
    public interface IDayStock
    {
        /// <summary>
        /// Identifier for the stock
        /// </summary>
        public StockId StockId { get; }
    }
}
