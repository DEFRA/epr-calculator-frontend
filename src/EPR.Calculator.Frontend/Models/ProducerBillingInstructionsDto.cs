using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Represents a single producer's billing instruction details.
    /// </summary>
    public record ProducerBillingInstructionsDto
    {
        /// <summary>
        /// Gets the name of the producer.
        /// </summary>
        [JsonPropertyName("producerName")]
        public string? ProducerName { get; init; }

        /// <summary>
        /// Gets the unique identifier for the producer.
        /// </summary>
        [JsonPropertyName("producerId")]
        public int ProducerId { get; init; }

        /// <summary>
        /// Gets the suggested billing instruction for the producer.
        /// </summary>
        [JsonPropertyName("suggestedBillingInstruction")]
        public string SuggestedBillingInstruction { get; init; } = string.Empty;

        /// <summary>
        /// Gets the suggested invoice amount for the producer.
        /// </summary>
        [JsonPropertyName("suggestedInvoiceAmount")]
        public decimal? SuggestedInvoiceAmount { get; init; }

        /// <summary>
        /// Gets the accept/reject status of the billing instruction.
        /// </summary>
        [JsonPropertyName("billingInstructionAcceptReject")]
        public string? BillingInstructionAcceptReject { get; init; }
    }
}