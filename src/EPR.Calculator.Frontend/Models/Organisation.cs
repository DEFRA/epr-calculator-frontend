using EPR.Calculator.Frontend.Enums;
using System.ComponentModel;

namespace EPR.Calculator.Frontend.Models
{
    public class Organisation
    {
        public int Id { get; set; }

        public string? OrganisationName { get; set; }

        public int OrganisationId { get; set; }

        public BillingInstruction? BillingInstruction { get; set; }

        public double InvoiceAmount { get; set; }

        public BillingStatus? Status { get; set; }
    }
}
