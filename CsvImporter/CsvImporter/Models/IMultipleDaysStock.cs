using System;

/// <summary>
/// Defines a grouper for days stock, as comparable
/// </summary>
namespace CsvImporter.Models
{
    interface IMultipleDaysStock : IDayStock, IComparable<MultipleDaysStock>
    {
    }
}
