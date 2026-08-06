using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace EPR.Calculator.Frontend.Helpers.Csv;

/// <summary>
///     Exists because CsvHelper.TypeConverterException appends extra information to the message.
/// </summary>
public class CsvValueException (ITypeConverter typeConverter, MemberMapData memberMapData, object? value, CsvContext context, string message)
    : TypeConverterException(typeConverter, memberMapData, value, context, message)
{
    public string DisplayMessage { get; } = message;
}
