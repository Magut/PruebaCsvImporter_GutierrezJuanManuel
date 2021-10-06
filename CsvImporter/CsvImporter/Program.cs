using System;
using System.IO;

namespace CsvImporter
{
    class Program
    {
        /// <summary>
        /// Main logic
        /// </summary>
        /// <param name="args">Arguments passed to the process</param>
        static void Main(string[] args)
        {
            InputFileLocation inputFileLocation;
            string filePath;

            (inputFileLocation, filePath) = GetArguments(args);


        }

        /// <summary>
        /// It parses the args of the process
        /// </summary>
        /// <param name="args">Arguments passed to the process</param>
        /// <returns>
        /// <see cref="InputFileLocation"/> with the location of the CSV file
        /// and <see cref="String"/> with the Path/URL of the file
        /// </returns>
        /// <exception cref="ArgumentException">If the arguments are not valid</exception>
        private static (InputFileLocation fileLocation, string path) GetArguments(string[] args)
        {
            // The process needs two and only two arguments
            if (args.Length != 2)
            {
                Console.WriteLine("Please enter valid arguments. Usage:");
                Console.WriteLine("\tCsvImporter <Azure|Localhost> <URL/Path>");
                throw new ArgumentException("The process needs 2 valid arguments. File location: <Azure|Localhost> and Path/URL of the file: <URL/Path>");
            }

            // Parse input arguments
            string location = args[0];
            string path = args[1];
            if (location.ToLower() != "azure" && location.ToLower() != "local" && location.ToLower() != "localhost")
            {
                Console.WriteLine("Please enter a valid location of the file as the first argument. Valid locations: Azure | Localhost");
                throw new ArgumentException("The argument location passed to the process is not valid");
            }

            InputFileLocation fileLocation = location.ToLower() == "azure" ? InputFileLocation.Azure : InputFileLocation.Localhost;

            if (fileLocation == InputFileLocation.Azure)
            {
                CheckValidUrl(path);
            }
            else
            {
                path = CheckValidLocalPath(path);
            }

            return (fileLocation, path);
        }

        /// <summary>
        /// Checks if the parameter <paramref name="url"/> looks like a valid URL
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns><see langword="true"/> if the <paramref name="url"/> looks like a valid URL, else <see langword="false"/></returns>
        private static bool CheckValidUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Checks if the parameter <paramref name="path"/> looks like a valid Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns><see langword="false"/> if the <paramref name="path"/> looks like a valid Path, else <see langword="false"/></returns>
        private static string CheckValidLocalPath(string path)
        {
            string fullPath;

            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                Console.WriteLine("Please enter a valid file path.");
                throw new ArgumentException("The argument file path passed to the process is not valid");
            }

            return fullPath;
        }
    }

    public enum InputFileLocation
    {
        Azure,
        Localhost
    }
}
