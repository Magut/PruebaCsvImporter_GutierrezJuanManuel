using System;

namespace CsvImporter.Models
{
    /// <summary>
    /// Grouper for days stock, as comparable
    /// </summary>
    public class MultipleDaysStock : IMultipleDaysStock
    {
        private readonly StockId _stockId;
        private DateTime _beginDate = DateTime.MinValue;
        private DateTime _endDate = DateTime.MaxValue;

        /// <summary>
        /// Constructor with the stock data
        /// </summary>
        /// <param name="stockId"><see cref="StockId"/> with the initialized data of the stock</param>
        public MultipleDaysStock(StockId stockId)
        {
            _stockId = stockId;
        }

        /// <summary>
        /// Constructor with the stock data and the dates
        /// </summary>
        /// <param name="stockId"><see cref="StockId"/> with the initialized data of the stock</param>
        /// <param name="beginDate">Date from which this stock is valid</param>
        /// <param name="endDate">Date until which this stock is valid</param>
        public MultipleDaysStock(StockId stockId, DateTime beginDate, DateTime endDate)
        {
            _stockId = stockId;
            _beginDate = beginDate.Date;
            _endDate = endDate.Date;
        }

        public StockId StockId => _stockId;
        public long PointOfSale => _stockId.PointOfSale;
        public long Product => _stockId.Product;
        public long Stock => _stockId.Stock;

        /// <summary>
        /// Date from which this stock status is valid
        /// </summary>
        public DateTime BeginDate
        {
            get
            {
                return _beginDate;
            }
            set
            {
                if (value > DateTime.MinValue && value < _endDate && value < DateTime.MaxValue)
                {
                    _beginDate = value;
                }
            }
        }

        /// <summary>
        /// Date until which this stock status is valid
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                if (value > DateTime.MinValue && value > _beginDate && value < DateTime.MaxValue)
                {
                    _endDate = value;
                }
            }
        }

        #region Comparers

        public int CompareTo(MultipleDaysStock other)
        {
            // If other is not a valid object reference, this instance is greater.
            if (other == null) return 1;

            return _beginDate == other._beginDate ? _endDate.CompareTo(other.EndDate) : _beginDate.CompareTo(other.BeginDate);
        }

        public static bool operator >(MultipleDaysStock operand1, MultipleDaysStock operand2)
        {
            return operand1.CompareTo(operand2) > 0;
        }

        public static bool operator <(MultipleDaysStock operand1, MultipleDaysStock operand2)
        {
            return operand1.CompareTo(operand2) < 0;
        }

        public static bool operator >=(MultipleDaysStock operand1, MultipleDaysStock operand2)
        {
            return operand1.CompareTo(operand2) >= 0;
        }

        public static bool operator <=(MultipleDaysStock operand1, MultipleDaysStock operand2)
        {
            return operand1.CompareTo(operand2) <= 0;
        }

        #endregion
    }
}
