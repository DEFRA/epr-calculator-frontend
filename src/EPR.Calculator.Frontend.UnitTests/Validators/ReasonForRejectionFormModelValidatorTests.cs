using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation.TestHelper;

namespace EPR.Calculator.Frontend.UnitTests.Validators;

[TestClass]
public class ReasonForRejectionFormModelValidatorTests
{
    private readonly ReasonForRejectionFormModelValidator _validator;

    public ReasonForRejectionFormModelValidatorTests()
    {
        _validator = new ReasonForRejectionFormModelValidator();
    }

    [TestMethod]
    public void Should_Have_Error_When_RunId_Is_Zero()
    {
        var model = new ReasonForRejectionFormModel { RunId = 0, Reason = "Valid reason" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.RunId)
            .WithErrorMessage(ErrorMessages.CalculatorRunIdGreaterThanZero);
    }

    [TestMethod]
    public void Should_Have_Error_When_RunId_Is_Negative()
    {
        var model = new ReasonForRejectionFormModel { RunId = -1, Reason = "Valid reason" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.RunId)
            .WithErrorMessage(ErrorMessages.CalculatorRunIdGreaterThanZero);
    }

    [TestMethod]
    public void Should_Not_Have_Error_When_RunId_Is_Positive()
    {
        var model = new ReasonForRejectionFormModel { RunId = 1, Reason = "Valid reason" };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.RunId);
    }

    [TestMethod]
    public void Should_Have_Error_When_Reason_Is_Null()
    {
        var model = new ReasonForRejectionFormModel { RunId = 1, Reason = null };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage(ErrorMessages.ReasonForRejectionRequired);
    }

    [TestMethod]
    public void Should_Have_Error_When_Reason_Is_Empty()
    {
        var model = new ReasonForRejectionFormModel { RunId = 1, Reason = string.Empty };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage(ErrorMessages.ReasonForRejectionRequired);
    }

    [TestMethod]
    public void Should_Have_Error_When_Reason_Is_Whitespace()
    {
        var model = new ReasonForRejectionFormModel { RunId = 1, Reason = "   " };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage(ErrorMessages.ReasonForRejectionRequired);
    }

    [TestMethod]
    public void Should_Have_Error_When_Reason_Exceeds_500_Characters()
    {
        var model = new ReasonForRejectionFormModel { RunId = 1, Reason = new string('A', 501) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage(ErrorMessages.ReasonMustNotExceed500Characters);
    }

    [TestMethod]
    public void Should_Not_Have_Error_When_Reason_Is_Exactly_500_Characters()
    {
        var model = new ReasonForRejectionFormModel { RunId = 1, Reason = new string('A', 500) };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [TestMethod]
    public void Should_Not_Have_Any_Errors_When_Model_Is_Valid()
    {
        var model = new ReasonForRejectionFormModel { RunId = 1, Reason = "Valid reason" };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
