using CsvImporter.Models;
using CsvImporter.Controllers.Writers;
using System;
using System.Collections.Concurrent;
using Xunit;

namespace CsvImporter.Test.WritersTests
{
    public class ImportedStockWriterTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ImportedStockWriterCtor_WithInvalidConnectionString_ThrowsException(string connectionStringName)
        {
            ConcurrentQueue<SingleDayStock> dataProcessedQueue = new();
            Flag processorFinishedProcessing = new();
            Flag finishedWriting = new();

            Assert.Throws<ArgumentNullException>(nameof(connectionStringName), () => new ImportedStockWriter(connectionStringName, dataProcessedQueue, processorFinishedProcessing, finishedWriting));
        }

        [Fact]
        public void ImportedStockWriterCtor_WithNullQueue_ThrowsException()
        {
            string connectionStringName = "validConnectionString";
            ConcurrentQueue<SingleDayStock> dataProcessedQueue = null;
            Flag processorFinishedProcessing = new();
            Flag finishedWriting = new();

            Assert.Throws<ArgumentNullException>(nameof(dataProcessedQueue), () => new ImportedStockWriter(connectionStringName, dataProcessedQueue, processorFinishedProcessing, finishedWriting));
        }

        [Fact]
        public void ImportedStockWriterCtor_WithNullProcessorFlag_ThrowsException()
        {
            string connectionStringName = "validConnectionString";
            ConcurrentQueue<SingleDayStock> dataProcessedQueue = new();
            Flag processorFinishedProcessing = null;
            Flag finishedWriting = new();

            Assert.Throws<ArgumentNullException>(nameof(processorFinishedProcessing), () => new ImportedStockWriter(connectionStringName, dataProcessedQueue, processorFinishedProcessing, finishedWriting));
        }

        [Fact]
        public void ImportedStockWriterCtor_WithNullFinishedWritingFlag_ThrowsException()
        {
            string connectionStringName = "validConnectionString";
            ConcurrentQueue<SingleDayStock> dataProcessedQueue = new();
            Flag processorFinishedProcessing = new();
            Flag finishedWriting = null;

            Assert.Throws<ArgumentNullException>(nameof(finishedWriting), () => new ImportedStockWriter(connectionStringName, dataProcessedQueue, processorFinishedProcessing, finishedWriting));
        }
    }
}
