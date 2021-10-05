using CsvImporter.Controllers.Readers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CsvImporter.Test.ReadersTests
{
    public class DataReaderTest
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
        public void DataReaderCtor_WithNullStream_ThrowsNullArgumentException()
        {
            StreamReader stream = null;
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

            Assert.Throws<ArgumentNullException>("stream", () => new DataReader(stream, queue));
        }

        [Fact]
        public void DataReaderCtor_WithNullQueue_ThrowsNullArgumentException()
        {
            string testFilePath = MakeTestFilePath(@".\DataReaderTest_TestFile_WithNullQueue.csv");
            CreateTestFile(testFilePath, new string[2] { _testFileRows[0], _testFileRows[1] });

            using (StreamReader stream = new StreamReader(testFilePath))
            {
                ConcurrentQueue<string> queue = null;

                Assert.Throws<ArgumentNullException>("queue", () => new DataReader(stream, queue));
            }
        }

        [Fact]
        public void StartReadingAsync_FileWithOnlyHeader_DoesNotEnqueuAnything()
        {
            string testFilePath = MakeTestFilePath(@".\DataReaderTest_TestFile_WithOnlyHeader.csv");
            CreateTestFile(testFilePath, new string[1] { _testFileRows[0] });

            using (var stream = new StreamReader(testFilePath))
            {
                var queue = new ConcurrentQueue<string>();
                var csvDataReader = new DataReader(stream, queue);

                Task t = Task.Run(() => csvDataReader.StartReadingAndEqueuingDataAsync());
                t.Wait();

                Assert.Empty(queue);
            }
        }

        [Fact]
        public void StartReadingAsync_FileWithHeaderAndData_EnqueuesOnlyData()
        {
            string testFilePath = MakeTestFilePath(@".\DataReaderTest_TestFile_WithHeaderAndData.csv");
            CreateTestFile(testFilePath, new string[2] { _testFileRows[0], _testFileRows[1] });

            using (var stream = new StreamReader(testFilePath))
            {
                var queue = new ConcurrentQueue<string>();
                var csvDataReader = new DataReader(stream, queue);

                Task t = Task.Run(() => csvDataReader.StartReadingAndEqueuingDataAsync());
                t.Wait();

                Assert.Single(queue);
            }
        }

        // TODO Quitar

        //[Fact]
        //public void StartReadingAsync_FileWithOnlyData_EnqueuesAllRows()
        //{
        //    string testFilePath = MakeTestFilePath(@".\testWithOnlyData.csv");
        //    CreateTestFile(testFilePath, new string[1] { _testFileRows[3] });

        //    using (var stream = new StreamReader(testFilePath))
        //    {
        //        var queue = new ConcurrentQueue<string>();
        //        var csvDataReader = new DataReader(stream, queue);

        //        csvDataReader.StartReadingAndEqueuingDataAsync();

        //        Assert.Single(queue);
        //    }
        //}

        #region Private Methods used by the Tests

        /// <summary>
        /// It returns the Full path for a test file
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <returns>Full path of the test file</returns>
        private string MakeTestFilePath(string fileName)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string newPath = Path.Combine(currentDirectory, fileName);
            return Path.GetFullPath(newPath);
        }

        /// <summary>
        /// It creates a Test CSV file
        /// </summary>
        /// <param name="path">Path of the test file</param>
        /// <param name="rowsToInsert">Rows to insert (the first one can be the header)</param>
        private void CreateTestFile(string path, string[] rowsToInsert)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var stream = File.Create(path))
            {
                ;
            }

            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                foreach (string row in rowsToInsert)
                {
                    streamWriter.WriteLine(row);
                }
            }
        }

        #endregion
    }
}
