using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EPR.Calculator.Frontend.Helpers.Csv;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Mappers;

public sealed class LapcapCsvMapper : ClassMap<SetLapcapDataRequest.LapcapValue>
{
    private const string CountryColumn = "country";
    private const string MaterialColumn = "material";
    private const string TotalCostColumn = "total_cost";

    public LapcapCsvMapper()
    {
        Map(m => m.Country).Name(CountryColumn);
        Map(m => m.Material).Name(MaterialColumn);
        Map(m => m.TotalCost).Name(TotalCostColumn).TypeConverter<TotalCostConverter>();
    }

    private sealed class TotalCostConverter : DefaultTypeConverter
    {
        /// <summary>
        ///     Used in place of a country or material that is missing from the row,
        ///     so that an error message can still be produced.
        /// </summary>
        private const string UnknownValue = "unknown";

        private const NumberStyles DefaultNumberStyles = NumberStyles.Currency;

        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            var culture = memberMapData.TypeConverterOptions.CultureInfo ?? row.Configuration.CultureInfo;

            if (decimal.TryParse(text, DefaultNumberStyles, culture, out var totalCost))
                return totalCost;

            var material = GetFieldOrUnknown(row, MaterialColumn);
            var country = GetFieldOrUnknown(row, CountryColumn);

            throw new CsvValueException(
                this,
                memberMapData,
                text ?? string.Empty,
                row.Context,
                $"The total cost for {material} in {country} is invalid. The cost can only contain the numbers, commas and decimal points.");
        }

        private static string GetFieldOrUnknown(IReaderRow row, string columnName)
        {
            return row.TryGetField<string>(columnName, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : UnknownValue;
        }
    }
}
