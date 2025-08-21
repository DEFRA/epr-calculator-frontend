using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class AcceptRejectConfirmationViewModelTests
    {
        [TestMethod]
        public void AcceptRejectConfirmationText_ReturnsAcceptText_WhenStatusIsAccepted()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                Status = BillingStatus.Accepted
            };
            Assert.AreEqual(CommonConstants.AcceptViewText, model.AcceptRejectConfirmationText);
        }

        [TestMethod]
        public void AcceptRejectConfirmationText_ReturnsRejectText_WhenStatusIsRejected()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                Status = BillingStatus.Rejected
            };
            Assert.AreEqual(CommonConstants.RejectViewText, model.AcceptRejectConfirmationText);
        }

        [TestMethod]
        public void AcceptRejectConfirmationText_ReturnsEmpty_WhenStatusIsOther()
        {
            var model = new AcceptRejectConfirmationViewModel
            {
                Status = BillingStatus.Noaction
            };
            Assert.AreEqual(string.Empty, model.AcceptRejectConfirmationText);

            model.Status = BillingStatus.Pending;
            Assert.AreEqual(string.Empty, model.AcceptRejectConfirmationText);
        }
    }
}