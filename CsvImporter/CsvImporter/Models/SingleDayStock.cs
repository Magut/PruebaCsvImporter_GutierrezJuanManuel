using System;
using System.Globalization;

namespace CsvImporter.Models
{
    /// <summary>
    /// Models a single day stock
    /// </summary>
    public class SingleDayStock : IDayStock
    {
        private readonly StockId _stockId;
        private readonly DateTime _date;

        /// <summary>
        /// Constructor with all the needed data
        /// </summary>
        /// <param name="stockId"><see cref="StockId"/> with the initialized data of the stock</param>
        /// <param name="date"></param>
        public SingleDayStock(StockId stockId, DateTime date)
        {
            _stockId = stockId;
            _date = date.Date;
        }

        /// <summary>
        /// Constructor with all the needed data in a <see cref="String"/>
        /// with the format of a CSV row
        /// </summary>
        /// <param name="csvRow">CSV row</param>
        /// <exception cref="ArgumentException">If the csvRow has no valid data</exception>
        public SingleDayStock(string csvRow, char separator = ';')
        {
            string[] csvRowFields = csvRow.Split(separator);
            if (!long.TryParse(csvRowFields[0], out long pointOfSale))
                throw new ArgumentException("The parameter is not a valid CSV row", nameof(csvRow));
            if (!long.TryParse(csvRowFields[1], out long product))
                throw new ArgumentException("The parameter is not a valid CSV row", nameof(csvRow));
            if (!DateTime.TryParseExact(csvRowFields[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                throw new ArgumentException("The parameter is not a valid CSV row", nameof(csvRow));
            if (!long.TryParse(csvRowFields[3], out long stock))
                throw new ArgumentException("The parameter is not a valid CSV row", nameof(csvRow));

            _stockId = new StockId(pointOfSale, product, stock);
            _date = date;
        }

        public StockId StockId => _stockId;
        public string PointOfSale => _stockId.PointOfSale;
        public string Product => _stockId.Product;
        public int Stock => _stockId.Stock;
        public DateTime Date => _date;
    }
}
