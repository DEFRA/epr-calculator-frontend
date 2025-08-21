using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation.TestHelper;

namespace EPR.Calculator.Frontend.UnitTests.Validators
{
    [TestClass]
    public class AcceptRejectConfirmationViewModelValidatorTests
    {
        private readonly AcceptRejectConfirmationViewModelValidator _validator;

        public AcceptRejectConfirmationViewModelValidatorTests()
        {
            _validator = new AcceptRejectConfirmationViewModelValidator();
        }

        [TestMethod]
        public void Should_Have_Error_When_CalculationRunId_Is_Zero_Or_Negative()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 0,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Accepted,
                ApproveData = true
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationRunId);

            model.CalculationRunId = -1;
            result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationRunId);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_CalculationRunId_Is_Positive()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Accepted,
                ApproveData = true
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationRunId);
        }

        [TestMethod]
        public void Should_Have_Error_When_CalculationRunName_Is_Empty_Or_Too_Long()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = string.Empty,
                Status = BillingStatus.Accepted,
                ApproveData = true
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationRunName);

            model.CalculationRunName = new string('A', 101);
            result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationRunName);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_CalculationRunName_Is_Valid()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Accepted,
                ApproveData = true
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationRunName);
        }

        [TestMethod]
        public void Should_Have_Error_When_Status_Is_Invalid()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = (BillingStatus)99,
                ApproveData = true
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Status);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Status_Is_Valid()
        {
            foreach (BillingStatus status in (BillingStatus[])System.Enum.GetValues(typeof(BillingStatus)))
            {
                var model = new AcceptRejectConfirmationViewModel
                {
                    CalculationRunId = 1,
                    CalculationRunName = "Valid Name",
                    Status = status,
                    ApproveData = true
                };
                var result = _validator.TestValidate(model);
                result.ShouldNotHaveValidationErrorFor(x => x.Status);
            }
        }

        [TestMethod]
        public void Should_Have_Error_When_ApproveData_Is_Null()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Accepted,
                ApproveData = null
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ApproveData);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_ApproveData_Is_Not_Null()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Accepted,
                ApproveData = true
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.ApproveData);
        }

        [TestMethod]
        public void Should_Require_Reason_When_Status_Is_Rejected()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Rejected,
                ApproveData = false,
                Reason = string.Empty,
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Reason);

            model.Reason = null;
            result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Reason);
        }

        [TestMethod]
        public void Should_Not_Allow_Reason_Over_500_Characters_When_Status_Is_Rejected()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Rejected,
                ApproveData = false,
                Reason = new string('A', 501)
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Reason);
        }

        [TestMethod]
        public void Should_Not_Have_Error_For_Reason_When_Status_Is_Not_Rejected()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Accepted,
                ApproveData = true,
                Reason = null
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Reason);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Reason_Is_Valid_And_Status_Is_Rejected()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Valid Name",
                Status = BillingStatus.Rejected,
                ApproveData = false,
                Reason = "Valid reason"
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Reason);
        }
    }
}