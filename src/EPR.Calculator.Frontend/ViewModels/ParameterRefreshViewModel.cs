using System.Text.Json.Serialization;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record ParameterRefreshViewModel : ViewModelCommonData
{
    [JsonPropertyName("schemeParameterTemplateValues")]
    public required List<SchemeParameterTemplateValue> ParameterTemplateValues { get; init; }
    [JsonPropertyName("parameterFileName")]
    public required string FileName { get; init; }
}
