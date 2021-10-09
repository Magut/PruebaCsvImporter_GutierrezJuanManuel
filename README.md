# CsvImporter - GutierrezJuanManuel
Console app responsible for importing from a CSV file into a Database

## Usage
**CsvImporter.exe \[Localhost/Azure\] \[FilePath\]**
*  First argument: Indicates whether the file is local or remote (from Azure)
*  Second argument: File path or URL of the file

## Details
The process reads from a file, process the data and insert it in the database. After inserting new data, it deletes all the previous imported rows, in a controlled way (it can be setted how many rows can be deleted per each delete statement).

The code is divided in modules:
1. Data readers: Controllers that reads from a file (*AzureFileReader, LocalFileReader, DataReader*).
2. Data processors: Controllers that process the data (*CsvDataProcessor*).
3. Data writers: Controllers that write the data into the database (*ImportedStockWriter*).

The app instances multiple processors and writers async tasks, that run simultanously.

## Pendings
Currently in development:
1. Improve the database connection builder
2. Add compression to the data to be sent to the database (in order to decrease the database connection and the network traffic)
