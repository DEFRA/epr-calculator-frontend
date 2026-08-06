using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EPR.Calculator.Frontend.Constants;
using MissingFieldException = CsvHelper.MissingFieldException;

namespace EPR.Calculator.Frontend.Helpers.Csv;

public static class CsvFileHelper
{
    public static CsvConfiguration DefaultCsvConfig => new (new CultureInfo("en-GB"))
    {
        // Skips rows where every field is empty.
        ShouldSkipRecord = args => args.Row.Parser.Record?.All(string.IsNullOrWhiteSpace) ?? true,

        // Normalizes header casing/spaces.
        PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant().Replace(" ", "_")
    };

    public static bool TryValidateFile([NotNullWhen(true)] IFormFile? fileUpload, out ImmutableList<string> errors)
    {
        var builder = ImmutableList.CreateBuilder<string>();

        if (fileUpload != null)
        {
            if (!fileUpload.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                builder.Add(ErrorMessages.FileMustBeCSV);

            if (fileUpload.Length > StaticHelpers.MaxFileSize)
                builder.Add(ErrorMessages.FileNotExceed50KB);
        }
        else
            builder.Add(ErrorMessages.FileNotSelected);

        errors = builder.ToImmutable();
        return errors.Count == 0;
    }

    public static async Task<CsvParseResult<T>> ParseAsync<TMapper, T>(IFormFile? fileUpload, CsvConfiguration? config = null, CancellationToken cancellationToken = default)
        where TMapper : ClassMap<T>
    {
        var contentErrors = ImmutableList.CreateBuilder<string>();

        try
        {
            if (!TryValidateFile(fileUpload, out var errors))
            {
                return new CsvParseResult<T>
                {
                    FileErrors = errors
                };
            }

            using var reader = new StreamReader(fileUpload.OpenReadStream());

            config ??= DefaultCsvConfig;
            config.ReadingExceptionOccurred = args =>
            {
                contentErrors.AddRange(Describe(args.Exception));
                return false;
            };

            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<TMapper>();

            var builder = ImmutableList.CreateBuilder<T>();

            await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
                builder.Add(record);

            return new CsvParseResult<T>
            {
                Records = builder.ToImmutable(),
                ContentErrors = contentErrors.ToImmutable()
            };
        }
        catch (CsvHelperException ex)
        {
            return new CsvParseResult<T>
            {
                ContentErrors = [..Describe(ex), ..contentErrors]
            };
        }
    }

    /// <summary>
    ///     Formats various CSV helper exceptions into user-friendly messages.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private static ImmutableList<string> Describe(CsvHelperException exception)
    {
        return exception switch
        {
            CsvValueException ex => [ex.DisplayMessage],
            HeaderValidationException ex => [..ex.InvalidHeaders.SelectMany(ih => ih.Names).Select(ih => $"Header '{ih}' is missing.")],
            TypeConverterException ex => [$"The value '{ex.Text}' in the '{ex.MemberMapData.Member?.Name}' column is invalid."],
            MissingFieldException => [$"Row {exception.Context?.Parser?.RawRow ?? 0} does not have a value for every column."],
            _ => [$"Row {exception.Context?.Parser?.RawRow ?? 0} could not be read."]
        };
    }
}
