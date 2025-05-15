namespace EPR.Calculator.Frontend.Models
{
    public record OrgProducerData
    {
        public string OrganisationName { get; set; }

        public string OrganisationID { get; set; }

        public string BillingInstructions { get; set; }

        public string InvoiceAmount { get; set; }

        public string Status { get; set; }
    }
}
