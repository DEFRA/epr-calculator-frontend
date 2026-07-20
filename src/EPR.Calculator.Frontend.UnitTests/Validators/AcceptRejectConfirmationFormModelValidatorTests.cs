using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation.TestHelper;

namespace EPR.Calculator.Frontend.UnitTests.Validators;

[TestClass]
public class AcceptRejectConfirmationFormModelValidatorTests
{
    private readonly AcceptRejectConfirmationFormModelValidator validator;

    public AcceptRejectConfirmationFormModelValidatorTests()
    {
        validator = new AcceptRejectConfirmationFormModelValidator();
    }

    [TestMethod]
    public void Should_Have_Error_When_CalculationRunId_Is_Zero_Or_Negative()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 0,
            Status = BillingStatus.Accepted,
            ApproveData = true
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.RunId);

        model = model with { RunId = -1 };
        result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.RunId);
    }

    [TestMethod]
    public void Should_Not_Have_Error_When_CalculationRunId_Is_Positive()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 1,
            Status = BillingStatus.Accepted,
            ApproveData = true
        };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.RunId);
    }

    [TestMethod]
    public void Should_Have_Error_When_Status_Is_Invalid()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 1,
            Status = (BillingStatus)99,
            ApproveData = true
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [TestMethod]
    public void Should_Not_Have_Error_When_Status_Is_Valid()
    {
        foreach (var status in (BillingStatus[])Enum.GetValues(typeof(BillingStatus)))
        {
            var model = new AcceptRejectConfirmationFormModel
            {
                RunId = 1,
                Status = status,
                ApproveData = true
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Status);
        }
    }

    [TestMethod]
    public void Should_Have_Error_When_ApproveData_Is_Null()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 1,
            Status = BillingStatus.Accepted,
            ApproveData = null
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ApproveData);
    }

    [TestMethod]
    public void Should_Not_Have_Error_When_ApproveData_Is_Not_Null()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 1,
            Status = BillingStatus.Accepted,
            ApproveData = true
        };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.ApproveData);
    }

    [TestMethod]
    public void Should_Require_Reason_When_Status_Is_Rejected()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 1,
            Status = BillingStatus.Rejected,
            ApproveData = false,
            Reason = string.Empty
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [TestMethod]
    public void Should_Not_Allow_Reason_Over_500_Characters_When_Status_Is_Rejected()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 1,
            Status = BillingStatus.Rejected,
            ApproveData = false,
            Reason = new string('A', 501)
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [TestMethod]
    public void Should_Not_Have_Error_For_Reason_When_Status_Is_Not_Rejected()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 1,
            Status = BillingStatus.Accepted,
            ApproveData = true,
            Reason = null
        };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [TestMethod]
    public void Should_Not_Have_Error_When_Reason_Is_Valid_And_Status_Is_Rejected()
    {
        var model = new AcceptRejectConfirmationFormModel
        {
            RunId = 1,
            Status = BillingStatus.Rejected,
            ApproveData = false,
            Reason = "Valid reason"
        };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }
}
