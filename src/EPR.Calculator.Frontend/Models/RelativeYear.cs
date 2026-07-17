using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.Models;

[ExcludeFromCodeCoverage]
[JsonConverter(typeof(RelativeYearSystemTextJsonConverter))]
public readonly record struct RelativeYear(int Value)
{
    public override string ToString() => Value.ToString();
    public string ToFinancialYear() => $"{Value}-{(Value + 1) % 100:D2}";

    public static implicit operator int(RelativeYear relativeYear) => relativeYear.Value;
    public static explicit operator RelativeYear(int value) => new(value);
}

[ExcludeFromCodeCoverage]
public class RelativeYearSystemTextJsonConverter : JsonConverter<RelativeYear>
{
    public override RelativeYear Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException("RelativeYear must be an integer");

        return new RelativeYear(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, RelativeYear value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
