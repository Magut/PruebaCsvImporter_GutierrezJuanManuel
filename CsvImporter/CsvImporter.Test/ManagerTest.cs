using CsvImporter.Controllers;
using System;
using Xunit;

namespace CsvImporter.Test
{
    public class ManagerTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ManagerCtor_WithInvalidFilePath_ThrowsException(string filePath)
        {
            InputFileLocation inputFileLocation = InputFileLocation.Localhost;

            Assert.Throws<ArgumentException>(() => new Manager(inputFileLocation, filePath));
        }

        [Fact]
        public void ManagerCtor_WithValidArguments_CreatesInstance()
        {
            string filePath = "C:/KindOfValidPath";
            InputFileLocation inputFileLocation = InputFileLocation.Localhost;

            Manager manager = new Manager(inputFileLocation, filePath);

            Assert.NotNull(manager);
        }
    }
}
