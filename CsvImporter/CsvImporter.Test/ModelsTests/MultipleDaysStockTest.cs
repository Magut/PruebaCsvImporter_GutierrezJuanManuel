using CsvImporter.Models;
using System;
using Xunit;

namespace CsvImporter.Test.ModelsTests
{
    public class MultipleDaysStockTest
    {
        [Theory]
        [InlineData(2021, 2, 15, 2021, 2, 15, 2021, 8, 1, 2021, 9, 30)]
        [InlineData(2021, 2, 15, 2021, 2, 15, 2021, 2, 15, 2021, 9, 30)]
        [InlineData(2021, 2, 15, 2021, 2, 17, 2021, 2, 16, 2021, 2, 16)]
        [InlineData(2020, 2, 15, 2021, 2, 17, 2021, 2, 16, 2021, 2, 16)]
        public void CompareTo_Greater_ReturnsNegative(int yearBeginDate1, int monthBeginDate1, int dayBeginDate1,
                                                      int yearEndDate1, int monthEndDate1, int dayEndDate1,
                                                      int yearBeginDate2, int monthBeginDate2, int dayBeginDate2,
                                                      int yearEndDate2, int monthEndDate2, int dayEndDate2)
        {
            DateTime beginDateInstance1 = new DateTime(yearBeginDate1, monthBeginDate1, dayBeginDate1);
            DateTime endDateInstance1 = new DateTime(yearEndDate1, monthEndDate1, dayEndDate1);
            DateTime beginDateInstance2 = new DateTime(yearBeginDate2, monthBeginDate2, dayBeginDate2);
            DateTime endDateInstance2 = new DateTime(yearEndDate2, monthEndDate2, dayEndDate2);
            StockId s1 = new StockId("1", "1", 1);
            StockId s2 = new StockId("2", "2", 2);
            MultipleDaysStock m1 = new MultipleDaysStock(s1, beginDateInstance1, endDateInstance1);
            MultipleDaysStock m2 = new MultipleDaysStock(s2, beginDateInstance2, endDateInstance2);

            int compareTo = m1.CompareTo(m2);

            Assert.True(compareTo < 0);
        }

        [Theory]
        [InlineData(2021, 2, 15, 2021, 2, 15, 2021, 1, 1, 2021, 1, 30)]
        [InlineData(2021, 2, 15, 2021, 2, 27, 2021, 2, 13, 2021, 2, 13)]
        [InlineData(2021, 2, 15, 2021, 2, 17, 2021, 2, 15, 2021, 2, 16)]
        [InlineData(2020, 2, 17, 2020, 2, 17, 2020, 2, 16, 2020, 2, 18)]
        public void CompareTo_Smaller_ReturnsPositive(int yearBeginDate1, int monthBeginDate1, int dayBeginDate1,
                                                      int yearEndDate1, int monthEndDate1, int dayEndDate1,
                                                      int yearBeginDate2, int monthBeginDate2, int dayBeginDate2,
                                                      int yearEndDate2, int monthEndDate2, int dayEndDate2)
        {
            DateTime beginDateInstance1 = new DateTime(yearBeginDate1, monthBeginDate1, dayBeginDate1);
            DateTime endDateInstance1 = new DateTime(yearEndDate1, monthEndDate1, dayEndDate1);
            DateTime beginDateInstance2 = new DateTime(yearBeginDate2, monthBeginDate2, dayBeginDate2);
            DateTime endDateInstance2 = new DateTime(yearEndDate2, monthEndDate2, dayEndDate2);
            StockId s1 = new StockId("1", "1", 1);
            StockId s2 = new StockId("2", "2", 2);
            MultipleDaysStock m1 = new MultipleDaysStock(s1, beginDateInstance1, endDateInstance1);
            MultipleDaysStock m2 = new MultipleDaysStock(s2, beginDateInstance2, endDateInstance2);

            int compareTo = m1.CompareTo(m2);

            Assert.True(compareTo > 0);
        }

        [Theory]
        [InlineData(2021, 2, 15, 2021, 2, 15, 2021, 2, 15, 2021, 2, 15)]
        [InlineData(2000, 1, 1, 2021, 12, 31, 2000, 1, 1, 2021, 12, 31)]
        public void CompareTo_Equal_ReturnsZero(int yearBeginDate1, int monthBeginDate1, int dayBeginDate1,
                                                int yearEndDate1, int monthEndDate1, int dayEndDate1,
                                                int yearBeginDate2, int monthBeginDate2, int dayBeginDate2,
                                                int yearEndDate2, int monthEndDate2, int dayEndDate2)
        {
            DateTime beginDateInstance1 = new DateTime(yearBeginDate1, monthBeginDate1, dayBeginDate1);
            DateTime endDateInstance1 = new DateTime(yearEndDate1, monthEndDate1, dayEndDate1);
            DateTime beginDateInstance2 = new DateTime(yearBeginDate2, monthBeginDate2, dayBeginDate2);
            DateTime endDateInstance2 = new DateTime(yearEndDate2, monthEndDate2, dayEndDate2);
            StockId s1 = new StockId("1", "1", 1);
            StockId s2 = new StockId("2", "2", 2);
            MultipleDaysStock m1 = new MultipleDaysStock(s1, beginDateInstance1, endDateInstance1);
            MultipleDaysStock m2 = new MultipleDaysStock(s2, beginDateInstance2, endDateInstance2);

            int compareTo = m1.CompareTo(m2);

            Assert.True(compareTo == 0);
        }

        [Fact]
        public void CompareTo_NullInstance_ReturnsPositive()
        {
            StockId s1 = new StockId("1", "1", 1);
            DateTime beginDateInstance1 = new DateTime(2021, 5, 5);
            DateTime endDateInstance1 = new DateTime(2021, 6, 6);
            MultipleDaysStock m1 = new MultipleDaysStock(s1, beginDateInstance1, endDateInstance1);
            MultipleDaysStock m2 = null;

            int compareTo = m1.CompareTo(m2);

            Assert.True(compareTo > 0);
        }
    }
}
