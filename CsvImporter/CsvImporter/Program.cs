using CsvImporter.Controllers;
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
            InputFileLocation inputFileLocation = InputFileLocation.Localhost;
            string filePath = string.Empty;
            try
            {
                (inputFileLocation, filePath) = GetArguments(args);
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine($"ERROR while initializing. Message: {argEx.Message}");
                Console.WriteLine("Please enter valid arguments. Usage:");
                Console.WriteLine("\tCsvImporter <Azure|Localhost> <URL/Path>");
                Environment.Exit(0);
            }

            Manager manager = new Manager(inputFileLocation, filePath);
            manager.Import();
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
                throw new ArgumentException("The process needs 2 valid arguments. File location: <Azure|Localhost> and Path/URL of the file: <URL/Path>");
            }

            // Parse input arguments
            string location = args[0];
            string path = args[1];
            if (location.ToLower() != "azure" && location.ToLower() != "local" && location.ToLower() != "localhost")
            {
                throw new ArgumentException("The argument location passed to the process is not valid");
            }

            InputFileLocation fileLocation = location.ToLower() == "azure" ? InputFileLocation.Azure : InputFileLocation.Localhost;

            return (fileLocation, path);
        }
    }

    public enum InputFileLocation
    {
        Azure,
        Localhost
    }
}
