using System.Collections.Immutable;

namespace EPR.Calculator.Frontend.Helpers.Csv;

public sealed record CsvParseResult<T>
{
    public bool IsSuccess => FileErrors.Count == 0 && ContentErrors.Count == 0;

    public ImmutableList<T> Records { get; init; } = [];

    /// <summary>
    ///     Problems with the uploaded file itself, such as the wrong file type or nothing being selected.
    /// </summary>
    public ImmutableList<string> FileErrors { get; init; } = [];

    /// <summary>
    ///     Problems with the data inside the file. These can only be corrected in the file itself,
    ///     so they are listed for the user to fix before uploading again.
    /// </summary>
    public ImmutableList<string> ContentErrors { get; init; } = [];
}
