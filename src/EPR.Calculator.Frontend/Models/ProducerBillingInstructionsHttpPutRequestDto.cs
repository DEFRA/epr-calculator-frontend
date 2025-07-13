namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Data transfer object for submitting billing instructions for one or more producers.
    /// Used in HTTP PUT requests to update the billing status of specified organisations.
    /// </summary>
    public class ProducerBillingInstructionsHttpPutRequestDto
    {
        /// <summary>
        /// Gets or sets the collection of organisation IDs for which the billing instructions apply.
        /// </summary>
        public required IEnumerable<int> OrganisationIds { get; set; }

        /// <summary>
        /// Gets or sets the billing status to be applied to the specified organisations.
        /// Expected values are typically "Accepted" or "Rejected".
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// Gets or sets the reason for rejection, if the status is "Rejected".
        /// This field is optional and may be null if the status is not "Rejected".
        /// </summary>
        public string? ReasonForRejection { get; set; }

        /// <summary>
        /// Authorisaion token for the request, typically used for API authentication.
        /// </summary>
        public string AuthorizationToken { get; set; }
    }
}
