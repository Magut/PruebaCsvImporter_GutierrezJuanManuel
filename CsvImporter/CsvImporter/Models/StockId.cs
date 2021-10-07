using System;

namespace CsvImporter.Models
{
    public class StockId : IEquatable<StockId>
    {
        private readonly string _pointOfSale;
        private readonly string _product;
        private readonly int _stock;

        /// <summary>
        /// Constructor with all the needed data
        /// </summary>
        /// <param name="pointOfSale"></param>
        /// <param name="product"></param>
        /// <param name="stock"></param>
        public StockId(string pointOfSale, string product, int stock)
        {
            _pointOfSale = pointOfSale;
            _product = product;
            _stock = stock;
        }

        /// <summary>
        /// Property for getting the value of the Point of sale initialized
        /// </summary>
        public string PointOfSale => _pointOfSale;
        /// <summary>
        /// Property for getting the value of the Product ID initialized
        /// </summary>
        public string Product => _product;
        /// <summary>
        /// Property for getting the value of the amount in stock initialized
        /// </summary>
        public int Stock => _stock;

        #region Equals operations

        public bool Equals(StockId other)
        {
            if (other == null)
                return false;

            return _pointOfSale == other.PointOfSale && _product == other.Product && _stock == other.Stock;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is not StockId stockIdObj)
                return false;
            else
                return Equals(stockIdObj);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(_pointOfSale, _product, _stock).GetHashCode();
        }

        public static bool operator ==(StockId stockId1, StockId stockId2)
        {
            if ((object)stockId1 == null || (object)stockId2 == null)
                return Object.Equals(stockId1, stockId2);

            return stockId1.Equals(stockId2);
        }

        public static bool operator !=(StockId stockId1, StockId stockId2)
        {
            if ((object)stockId1 == null || (object)stockId2 == null)
                return !Object.Equals(stockId1, stockId2);

            return !(stockId1.Equals(stockId2));
        }

        #endregion
    }
}
