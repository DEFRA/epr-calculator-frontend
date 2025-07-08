using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.Models
{
    public record ProducerBillingInstructionsDto
    {
        [JsonPropertyName("producerName")]
        public string? ProducerName { get; init; }

        [JsonPropertyName("producerId")]
        public int ProducerId { get; init; }

        [JsonPropertyName("suggestedBillingInstruction")]
        public string SuggestedBillingInstruction { get; init; } = string.Empty;

        [JsonPropertyName("suggestedInvoiceAmount")]
        public decimal SuggestedInvoiceAmount { get; init; }

        [JsonPropertyName("billingInstructionAcceptReject")]
        public string? BillingInstructionAcceptReject { get; init; }
    }
}