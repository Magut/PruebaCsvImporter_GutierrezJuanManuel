using CsvImporter.Models;
using System;
using Xunit;

/// <summary>
/// Models a single day stock
/// </summary>
namespace CsvImporter.Test.ModelsTests
{
    public class SingleDayStockTest
    {
        [Theory]
        [InlineData("121017;17240503103734;2019-08-17;2")]
        [InlineData("1;2;2021-12-31;2", ';')]
        [InlineData("1000,2000,2021-12-01,2", ',')]
        public void SingleDayStockCtor_WithValidCsvStringsAndSeparators_ParsesStockDataOk(string csvRow, char separator = ';')
        {
            SingleDayStock stock = new SingleDayStock(csvRow, separator);

            Assert.NotNull(stock.StockId);
        }

        [Theory]
        [InlineData("121017;17240503103734;2019-12-01;2")]
        [InlineData("1;2;2019-12-01;2", ';')]
        [InlineData("1000,2000,2019-12-01,2", ',')]
        public void SingleDayStockCtor_WithValidCsvStringAndSeparator_ParsesDateOk(string csvRow, char separator = ';')
        {
            SingleDayStock stock = new SingleDayStock(csvRow, separator);

            Assert.NotNull(stock.StockId);
            Assert.True(stock.Date == new DateTime(2019, 12, 1));
        }
    }
}
