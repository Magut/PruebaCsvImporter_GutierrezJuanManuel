using CsvImporter.Controllers.Readers;
using System;
using System.Collections.Concurrent;
using System.IO;
using Xunit;

namespace CsvImporter.Test.ReadersTests
{
    public class AzureFileReaderTest
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
        public void AzureFileReaderCtor_WithNullFileUrl_ThrowsArgumentException()
        {
            Uri fileUrl = null;
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

            Assert.Throws<ArgumentNullException>("fileUrl", () => new AzureFileReader(fileUrl, queue));
        }

        [Fact]
        public void AzureFileReaderCtor_WithNullQueue_ThrowsArgumentException()
        {
            Uri fileUrl = new Uri("http://fakeuri");
            ConcurrentQueue<string> queue = null;

            Assert.Throws<ArgumentNullException>("queue", () => new AzureFileReader(fileUrl, queue));
        }

        [Fact]
        public void AzureFileReaderCtor_WithValidArguments_CreatesInstance()
        {
            Uri fileUrl = new Uri("http://fakeuri");
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

            AzureFileReader reader = new AzureFileReader(fileUrl, queue);

            Assert.NotNull(reader);
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
