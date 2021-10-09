using CsvImporter.Controllers.Readers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        [Fact]
        public void ReadFile_TestFileWithCancellation_EnqueuesData()
        {
            Uri testFileUrl = new Uri(@"https://storage10082020.blob.core.windows.net/y9ne9ilzmfld/Stock.CSV");
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
            // Initializes a CancellationToken that will throw a cancellation after 4 seconds
            CancellationTokenSource cancellSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellSource.Token;
            AzureFileReader reader = new AzureFileReader(testFileUrl, queue);

            Task tRead = Task.Run(async () => await reader.ReadFileAndEnqueueDataAsync(cancellationToken), cancellationToken);
            Task tCancell = Task.Run(() => { Thread.Sleep(5000); cancellSource.Cancel(); });
            Task.WaitAll(new Task[] { tRead, tCancell });

            Assert.NotEmpty(queue);
        }
    }
}
