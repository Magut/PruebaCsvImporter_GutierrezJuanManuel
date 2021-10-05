using System;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace CsvImporter.Controllers
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// Returns the full ConnectionString for a certain ConnectionString name
        /// </summary>
        /// <param name="name">ConnectionString name</param>
        /// <returns>Full ConnectionString</returns>
        public static string GetConnectionString(string name)
        {
            // TODO Pasar a ConfigurationManager
            if (name == "Stock")
                return @"Server=.\Exercises;Database=Stock;Trusted_Connection=True;";
            else
                return @"Data Source=:memory:;Version=3;New=True;";
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        /// <summary>
        /// Returns the full ConnectionString for a certain ConnectionString name
        /// </summary>
        /// <param name="name">ConnectionString name</param>
        /// <returns>Full ConnectionString</returns>
        public static string GetProvider(string name)
        {
            // TODO Pasar a ConfigurationManager
            if (name == "Stock")
                return "System.Data.SqlClient";
            else
                return "System.Data.SQLite";
            return ConfigurationManager.ConnectionStrings[name].ProviderName;
        }

        /// <summary>
        /// Creates a DbConnection for any provider, given a <paramref name="connectionStringName"/> included in the App.config file
        /// </summary>
        /// <param name="connectionStringName">Name of the connection string in the App.config file</param>
        /// <returns><see cref="DbConnection"/> on success or <see langword="null"/> null on failure</returns>
        public static DbConnection CreateDbConnection(string connectionStringName)
        {
            // Assume failure.
            DbConnection connection = null;

            string connectionString = GetConnectionString(connectionStringName);
            string provider = GetProvider(connectionStringName);

            Debug.WriteLine($"ConnectionString = {connectionString}");
            Debug.WriteLine($"Provider = {provider}");

            // Create the DbProviderFactory and DbConnection.
            if (connectionString != null && provider != null)
            {
                try
                {
                    //DbProviderFactory factory = DbProviderFactories.GetFactory(provider);
                    //connection = factory.CreateConnection();
                    //connection.ConnectionString = connectionString;

                    // TODO Generar dinámicamente o quitar
                    if (connectionStringName == "Stock")
                        connection = new SqlConnection(connectionString);
                    else
                        connection = new SQLiteConnection(connectionString);
                }
                catch (Exception ex)
                {
                    // Set the connection to null if it was created.
                    if (connection != null)
                    {
                        connection = null;
                    }

                    Console.WriteLine(ex.Message);
                }
            }

            // Return the connection.
            return connection;
        }

        /// <summary>
        /// Creates a <see cref="DbConnection"/> for any provider, given an specific <paramref name="provider"/> name and <paramref name="connectionString"/>
        /// </summary>
        /// <param name="provider">Provider name</param>
        /// <param name="connectionString">Full connection string</param>
        /// <returns><see cref="DbConnection"/> on success or <see langword="null"/> null on failure</returns>
        public static DbConnection CreateDbConnection(string provider, string connectionString)
        {
            // Assume failure.
            DbConnection connection = null;

            // Create the DbProviderFactory and DbConnection.
            if (connectionString != null)
            {
                try
                {
                    DbProviderFactory factory =
                        DbProviderFactories.GetFactory(provider);

                    connection = factory.CreateConnection();
                    connection.ConnectionString = connectionString;
                }
                catch (Exception ex)
                {
                    // Set the connection to null if it was created.
                    if (connection != null)
                    {
                        connection = null;
                    }
                    Console.WriteLine(ex.Message);
                }
            }
            // Return the connection.
            return connection;
        }
    }
}
