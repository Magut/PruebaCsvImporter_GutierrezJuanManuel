using CsvImporter.Controllers.Processors;
using CsvImporter.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace CsvImporter.Test.ProcessorsTests
{
    public class CsvDataProcessorTests
    {
        private static readonly string[] _testFileRows = new string[7]
        {
            "PointOfSale;Product;Date;Stock",
            "121017;17240503103734;2019-08-17;2",
            "121017;17240503103734;2019-08-18;2",
            "121017;17240503103734;2019-08-19;2",
            "121017;17240503103734;2019-08-20;2",
            "121017;17240503103734;2019-08-21;2",
            "121017;17240503103734;2019-08-22;2"
        };

        [Fact]
        public void CsvDataProcessorCtor_WithNullReaderQueue_ThrowsArgumentNullException()
        {
            ConcurrentQueue<string> readerQueue = null;
            ConcurrentQueue<SingleDayStock> dataToWriteQueue = new ConcurrentQueue<SingleDayStock>();
            Flag readerFinishedReading = new Flag();
            Flag finishedEqueuingDataForWriter = new Flag();

            Assert.Throws<ArgumentNullException>("readerQueue",
                                                 () => new CsvDataProcessor(readerQueue,
                                                                            dataToWriteQueue,
                                                                            readerFinishedReading,
                                                                            finishedEqueuingDataForWriter));
        }

        [Fact]
        public void CsvDataProcessorCtor_WithNullDataToWriteQueue_ThrowsArgumentNullException()
        {
            ConcurrentQueue<string> readerQueue = new ConcurrentQueue<string>();
            ConcurrentQueue<SingleDayStock> dataToWriteQueue = null;
            Flag readerFinishedReading = new Flag();
            Flag finishedEqueuingDataForWriter = new Flag();

            Assert.Throws<ArgumentNullException>("dataToWriteQueue",
                                                 () => new CsvDataProcessor(readerQueue,
                                                                            dataToWriteQueue,
                                                                            readerFinishedReading,
                                                                            finishedEqueuingDataForWriter));
        }

        [Fact]
        public void CsvDataProcessorCtor_WithNullReaderFinishedReadingFlag_ThrowsArgumentNullException()
        {
            ConcurrentQueue<string> readerQueue = new ConcurrentQueue<string>();
            ConcurrentQueue<SingleDayStock> dataToWriteQueue = new ConcurrentQueue<SingleDayStock>();
            Flag readerFinishedReading = null;
            Flag finishedEqueuingDataForWriter = new Flag();

            Assert.Throws<ArgumentNullException>("readerFinishedReading",
                                                 () => new CsvDataProcessor(readerQueue,
                                                                            dataToWriteQueue,
                                                                            readerFinishedReading,
                                                                            finishedEqueuingDataForWriter));
        }

        [Fact]
        public void CsvDataProcessorCtor_WithNullFinishedEqueuingDataForWriterFlag_ThrowsArgumentNullException()
        {
            ConcurrentQueue<string> readerQueue = new ConcurrentQueue<string>();
            ConcurrentQueue<SingleDayStock> dataToWriteQueue = new ConcurrentQueue<SingleDayStock>();
            Flag readerFinishedReading = new Flag();
            Flag finishedEnqueuingDataForWriter = null;

            Assert.Throws<ArgumentNullException>("finishedEnqueuingDataForWriter",
                                                 () => new CsvDataProcessor(readerQueue,
                                                                            dataToWriteQueue,
                                                                            readerFinishedReading,
                                                                            finishedEnqueuingDataForWriter));
        }

        [Fact]
        public void StartProcessingAndEnqueuingDataAsync_WithEmptyAndFinishedInputQueue_EnquesNothing()
        {
            ConcurrentQueue<string> readerQueue = new ConcurrentQueue<string>();
            ConcurrentQueue<SingleDayStock> dataToWriteQueue = new ConcurrentQueue<SingleDayStock>();
            Flag readerFinishedReading = new Flag() { Event = true };
            Flag finishedEqueuingDataForWriter = new Flag();
            CsvDataProcessor processor = new CsvDataProcessor(readerQueue,
                                                              dataToWriteQueue,
                                                              readerFinishedReading,
                                                              finishedEqueuingDataForWriter);

            Task t = Task.Run(() => processor.StartProcessingAndEnqueuingDataAsync());
            t.Wait();

            Assert.True(finishedEqueuingDataForWriter.Event);
            Assert.Empty(dataToWriteQueue);
        }

        [Fact]
        public void StartProcessingAndEnqueuingDataAsync_WithEmptyAndDelayedFinishedInputQueue_EnquesNothingAndWaitsForFinishedFlag()
        {
            ConcurrentQueue<string> readerQueue = new ConcurrentQueue<string>();
            ConcurrentQueue<SingleDayStock> dataToWriteQueue = new ConcurrentQueue<SingleDayStock>();
            Flag readerFinishedReading = new Flag();
            Flag finishedEqueuingDataForWriter = new Flag();
            CsvDataProcessor processor = new CsvDataProcessor(readerQueue,
                                                              dataToWriteQueue,
                                                              readerFinishedReading,
                                                              finishedEqueuingDataForWriter);
            Stopwatch timeMeassure = new Stopwatch();

            // Task that executes the processor
            Task t1 = Task.Run(async () => { timeMeassure.Start(); await processor.StartProcessingAndEnqueuingDataAsync(); });
            // Task that marks the readerFinishedReading event after 5 seconds
            Task t2 = Task.Run(async () => { await Task.Delay(5000); readerFinishedReading.Event = true; });

            t1.Wait();
            timeMeassure.Stop();
            Debug.WriteLine($"Time Elapsed: {timeMeassure.ElapsedMilliseconds} mseg");

            Assert.True(finishedEqueuingDataForWriter.Event);
            Assert.Empty(dataToWriteQueue);
            Assert.True(timeMeassure.ElapsedMilliseconds > 4500);
        }

        [Fact]
        public void StartProcessingAndEnqueuingDataAsync_WithDataInInputQueue_EnquesData()
        {
            ConcurrentQueue<string> readerQueue = new ConcurrentQueue<string>();
            readerQueue.Enqueue(_testFileRows[1]);
            ConcurrentQueue<SingleDayStock> dataToWriteQueue = new ConcurrentQueue<SingleDayStock>();
            Flag readerFinishedReading = new Flag() { Event = true };
            Flag finishedEqueuingDataForWriter = new Flag();

            CsvDataProcessor processor = new CsvDataProcessor(readerQueue,
                                                              dataToWriteQueue,
                                                              readerFinishedReading,
                                                              finishedEqueuingDataForWriter);

            Task t = Task.Run(() => processor.StartProcessingAndEnqueuingDataAsync());
            t.Wait();

            Assert.True(finishedEqueuingDataForWriter.Event);
            Assert.Empty(readerQueue);
            Assert.Single(dataToWriteQueue);
        }
    }
}
