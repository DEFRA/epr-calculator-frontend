using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.Converters;

/// <summary>
///     Reads <see cref="DateTime" /> values as UTC.
/// </summary>
/// <remarks>
///     The API deals exclusively in UTC, but doesn't always include an offset in its payloads (e.g.
///     <c>2026-09-10T10:57:49</c>), which <see cref="System.Text.Json" /> would otherwise read as
///     <see cref="DateTimeKind.Unspecified" />. Anything downstream that then formats the value with a time zone
///     would silently shift the timestamp, so values without an offset are assumed to be UTC and values with one are
///     converted to UTC.
///     <para>
///         Writing is left as-is: the framework's default already emits a round-trippable ISO 8601 value,
///         including the offset when the <see cref="DateTime.Kind" /> is known.
///     </para>
/// </remarks>
public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();

        return value.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => value
        };
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
