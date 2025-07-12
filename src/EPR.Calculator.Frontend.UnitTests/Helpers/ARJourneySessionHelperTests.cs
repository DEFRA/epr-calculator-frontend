using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.UnitTests.Mocks;

namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    [TestClass]
    public class ARJourneySessionHelperTests
    {
        // Static readonly arrays for assertions
        private static readonly int[] MergedProducerIds = { 1, 2, 3, 4 };
        private static readonly int[] RemainingAfterRemove = { 1, 3 };

        private const string SessionKey = SessionConstants.ProducerIds;
        private MockHttpSession _session;

        public ARJourneySessionHelperTests()
        {
            _session = new MockHttpSession();
        }

        [TestMethod]
        public void AddToSession_AddsProducerIdsToEmptySession()
        {
            // Arrange
            var producerIds = new List<int> { 1, 2, 3 };

            // Act
            ARJourneySessionHelper.AddToSession(_session, producerIds);

            // Assert
            var result = ARJourneySessionHelper.GetFromSession(_session);
            CollectionAssert.AreEquivalent(producerIds, result.ToList());
        }

        [TestMethod]
        public void AddToSession_MergesProducerIdsWithExisting()
        {
            // Arrange
            var existing = new List<int> { 1, 2 };
            ARJourneySessionHelper.AddToSession(_session, existing);

            var toAdd = new List<int> { 2, 3, 4 };

            // Act
            ARJourneySessionHelper.AddToSession(_session, toAdd);

            // Assert
            var result = ARJourneySessionHelper.GetFromSession(_session);
            CollectionAssert.AreEquivalent(MergedProducerIds, result.ToList());
        }

        [TestMethod]
        public void RemoveFromSession_RemovesProducerIds()
        {
            // Arrange
            var existing = new List<int> { 1, 2, 3, 4 };
            ARJourneySessionHelper.AddToSession(_session, existing);

            var toRemove = new List<int> { 2, 4 };

            // Act
            ARJourneySessionHelper.RemoveFromSession(_session, toRemove);

            // Assert
            var result = ARJourneySessionHelper.GetFromSession(_session);
            CollectionAssert.AreEquivalent(RemainingAfterRemove, result.ToList());
        }

        [TestMethod]
        public void GetFromSession_ReturnsProducerIds()
        {
            // Arrange
            var existing = new List<int> { 5, 6, 7 };
            ARJourneySessionHelper.AddToSession(_session, existing);

            // Act
            var result = ARJourneySessionHelper.GetFromSession(_session);

            // Assert
            CollectionAssert.AreEquivalent(existing, result.ToList());
        }

        [TestMethod]
        public void GetFromSession_ReturnsEmptySetIfNone()
        {
            // Act
            var result = ARJourneySessionHelper.GetFromSession(_session);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ClearAllFromSession_RemovesSessionKey()
        {
            // Arrange
            var existing = new List<int> { 1, 2, 3 };
            ARJourneySessionHelper.AddToSession(_session, existing);

            // Act
            ARJourneySessionHelper.ClearAllFromSession(_session);

            // Assert
            var result = ARJourneySessionHelper.GetFromSession(_session);
            Assert.AreEqual(0, result.Count);
        }
    }
}