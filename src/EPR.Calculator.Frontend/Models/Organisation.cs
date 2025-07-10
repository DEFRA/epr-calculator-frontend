using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.Models
{
    public record Organisation
    {
        public int Id { get; init; }

        public string? OrganisationName { get; init; }

        public int OrganisationId { get; init; }

        public BillingInstruction? BillingInstruction { get; init; }

        public decimal InvoiceAmount { get; init; }

        public BillingStatus? Status { get; init; }
    }
}
