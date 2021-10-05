using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using CsvImporter.Controllers;
using Xunit;

namespace CsvImporter.Test
{
    public class DatabaseHelperTest
    {
        [Fact]
        public void GetConnectionString_GivenValidName_ReturnsValidConnectionString()
        {
            string name = "Stock";

            string connectionString = DatabaseHelper.GetConnectionString(name);

            Assert.Contains("Server", connectionString);
        }

        [Fact]
        public void GetProvider_GivenValidName_ReturnsValidProvider()
        {
            string name = "Stock";

            string provider = DatabaseHelper.GetProvider(name);

            Assert.True(provider.Length > 0);
        }

        [Fact]
        public void CreateDbConnection_GivenValidName_ReturnsValidConnection()
        {
            string validConnectionStringName = "Stock";
            
            DbConnection conn = DatabaseHelper.CreateDbConnection(validConnectionStringName);

            Assert.NotNull(conn);
        }
    }
}
