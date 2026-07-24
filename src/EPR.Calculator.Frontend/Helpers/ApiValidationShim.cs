using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Helpers;

/// <summary>
///     EPR.Calculator.API currently returns inconsistent error structures for <c>400 Bad Request</c>.
///     It returns a mix of the standard ASPNET <see cref="ValidationProblemDetails" /> structure and arrays of custom
///     error object types.
///     This shim bridges such gaps until the API can be refactored to be more consistent.
/// </summary>
public static class ApiValidationShim
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    ///     Parses 400 Bad Request response JSON to the specified custom error type OR the more general
    ///     <see cref="ValidationErrorDto" /> type, depending on the JSON data structure.
    /// </summary>
    public static (ImmutableArray<TError>, ImmutableArray<ValidationErrorDto>) Parse<TError>(string jsonString)
    {
        ImmutableArray<TError> customErrors = [];
        ImmutableArray<ValidationErrorDto> basicErrors = [];

        using var doc = JsonDocument.Parse(jsonString);
        var root = doc.RootElement;

        if (TryParseAsProblemDetails(root, out var problemDetails))
        {
            basicErrors = problemDetails.Errors
                .SelectMany(kv => kv.Value.Select(e => new ValidationErrorDto { ErrorMessage = e }))
                .ToImmutableArray();
        }
        else if (root.ValueKind == JsonValueKind.Array)
            customErrors = root.Deserialize<ImmutableArray<TError>>()!;
        else
            basicErrors = [ new ValidationErrorDto { ErrorMessage = "An error occurred." } ];

        return (customErrors, basicErrors);
    }

    private static bool TryParseAsProblemDetails(JsonElement root, [NotNullWhen(true)] out ValidationProblemDetails? problemDetails)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            var type = root
                .EnumerateObject()
                .Where(kv =>
                    kv.Name.Equals("type", StringComparison.OrdinalIgnoreCase)
                    && kv.Value.ValueKind == JsonValueKind.String)
                .Select(kv => kv.Value.GetString())
                .FirstOrDefault() ?? "";

            // Matching on the 'type' string property gives a decent indication whether the
            // JSON object structure matches what is expected for ValidationProblemDetails.
            if (type.StartsWith("https://tools.ietf.org/html/rfc9110", StringComparison.OrdinalIgnoreCase))
            {
                problemDetails = root.Deserialize<ValidationProblemDetails>(JsonSerializerOptions)!;
                return true;
            }
        }

        problemDetails = null;
        return false;
    }
}
