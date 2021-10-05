using CsvImporter.Controllers.Readers;
using CsvImporter.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CsvImporter.Test.ReadersTests
{
    public class LocalFileReaderTest
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
        public void LocalFileReaderCtor_WithoutFlagWithNullLocalFilePath_ThrowsArgumentException()
        {
            string localFilePath = null;
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

            Assert.Throws<ArgumentNullException>("localFilePath", () => new LocalFileReader(localFilePath, queue));
        }

        [Fact]
        public void LocalFileReaderCtor_WithoutFlagWithInvalidLocalFilePath_ThrowsArgumentException()
        {
            string localFilePath = String.Empty;
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

            Assert.Throws<ArgumentException>("localFilePath", () => new LocalFileReader(localFilePath, queue));
        }

        [Fact]
        public void LocalFileReaderCtor_WithoutFlagWithNullQueue_ThrowsArgumentException()
        {
            string localFilePath = "test.csv";
            ConcurrentQueue<string> queue = null;

            Assert.Throws<ArgumentNullException>("queue", () => new LocalFileReader(localFilePath, queue));
        }

        [Fact]
        public void LocalFileReaderCtor_WithFlag_ThrowsArgumentException()
        {
            string localFilePath = "test.csv";
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
            Flag flag = new Flag();

            LocalFileReader fileReader = new LocalFileReader(localFilePath, queue, flag);

            Assert.NotNull(fileReader);
            Assert.False(flag.Event);
        }

        [Fact]
        public void ReadFile_FileWithOnlyHeader_DoesNotEnqueuAnything()
        {
            string testFilePath = MakeTestFilePath(@".\LocalFileReaderTest_TestFile_WithOnlyHeader.csv");
            CreateTestFile(testFilePath, new string[1] { _testFileRows[0] });
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
            LocalFileReader fileReader = new LocalFileReader(testFilePath, queue);

            Task t = Task.Run(() => fileReader.ReadFileAndEnqueueDataAsync());
            t.Wait();

            Assert.Empty(queue);
        }

        [Fact]
        public void ReadFile_FileWithHeaderAndData_DoesNotEnqueuAnything()
        {
            string testFilePath = MakeTestFilePath(@".\LocalFileReaderTest_TestFile_WithHeaderAndData.csv");
            CreateTestFile(testFilePath, new string[2] { _testFileRows[0], _testFileRows[5] });
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
            LocalFileReader fileReader = new LocalFileReader(testFilePath, queue);

            Task t = Task.Run(() => fileReader.ReadFileAndEnqueueDataAsync());
            t.Wait();

            Assert.Single(queue);
        }

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
