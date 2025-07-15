using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Services;

/// <summary>
/// Defines operations for accepting or rejecting producer billing instructions
/// for a specific calculation run.
/// </summary>
public interface IBillingInstructionsApiService
{
    /// <summary>
    /// Sends a request to accept or reject billing instructions for the specified calculation run.
    /// </summary>
    /// <param name="calculationRunId">The ID of the calculation run to update billing instructions for.</param>
    /// <param name="request">
    /// The billing instructions request containing organisation IDs, status, and an optional reason for rejection.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean indicating
    /// whether the operation was successful.
    /// </returns>
    Task<bool> PutAcceptRejectBillingInstructions(int calculationRunId, ProducerBillingInstructionsHttpPutRequestDto requestDto);
}