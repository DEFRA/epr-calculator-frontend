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

        [TestMethod]
        public void GetSelectionSummary_NoneSelected_PromptsToSearch()
        {
            Assert.AreEqual(
                "No organisations selected. Use search to find and select an organisation.",
                ARJourneySessionHelper.GetSelectionSummary(_session));
        }

        [TestMethod]
        public void GetSelectionSummary_OneSelectedWithName_NamesOrganisation()
        {
            ARJourneySessionHelper.AddToSession(_session, [100123]);
            ARJourneySessionHelper.AddNamesToSession(_session, [KeyValuePair.Create(100123, "Acme Ltd")]);

            Assert.AreEqual(
                "1 organisation selected: Acme Ltd (100123)",
                ARJourneySessionHelper.GetSelectionSummary(_session));
        }

        [TestMethod]
        public void GetSelectionSummary_OneSelectedWithoutName_ShowsEmptyNameAndId()
        {
            ARJourneySessionHelper.AddToSession(_session, [100123]);

            Assert.AreEqual("1 organisation selected:  (100123)", ARJourneySessionHelper.GetSelectionSummary(_session));
        }

        [TestMethod]
        public void GetSelectionSummary_ManySelected_FormatsCount()
        {
            ARJourneySessionHelper.AddToSession(_session, Enumerable.Range(1, 3123));

            Assert.AreEqual("3,123 organisations selected", ARJourneySessionHelper.GetSelectionSummary(_session));
        }

        [TestMethod]
        public void GetSelectionSummary_AfterDeselectingDownToOne_NamesRemainingOrganisation()
        {
            ARJourneySessionHelper.AddToSession(_session, [1, 2]);
            ARJourneySessionHelper.AddNamesToSession(_session, [KeyValuePair.Create(1, "First"), KeyValuePair.Create(2, "Second")]);

            ARJourneySessionHelper.RemoveFromSession(_session, [1]);

            Assert.AreEqual("1 organisation selected: Second (2)", ARJourneySessionHelper.GetSelectionSummary(_session));
            Assert.IsNull(ARJourneySessionHelper.GetNameFromSession(_session, 1));
        }

        [TestMethod]
        public void AddNamesToSession_IgnoresBlankNames()
        {
            ARJourneySessionHelper.AddNamesToSession(_session, [KeyValuePair.Create(1, " ")]);

            Assert.IsNull(ARJourneySessionHelper.GetNameFromSession(_session, 1));
        }

        [TestMethod]
        public void AddNamesToSession_ReplacesChangedName()
        {
            ARJourneySessionHelper.AddNamesToSession(_session, [KeyValuePair.Create(1, "Old name")]);

            ARJourneySessionHelper.AddNamesToSession(_session, [KeyValuePair.Create(1, "New name")]);

            Assert.AreEqual("New name", ARJourneySessionHelper.GetNameFromSession(_session, 1));
        }

        [TestMethod]
        public void AddNamesToSession_KeepsUnchangedName()
        {
            ARJourneySessionHelper.AddNamesToSession(_session, [KeyValuePair.Create(1, "Same name")]);

            ARJourneySessionHelper.AddNamesToSession(_session, [KeyValuePair.Create(1, "Same name")]);

            Assert.AreEqual("Same name", ARJourneySessionHelper.GetNameFromSession(_session, 1));
        }

        [TestMethod]
        public void RemoveFromSession_WhenNoNamesStored_RemovesIds()
        {
            ARJourneySessionHelper.AddToSession(_session, [1, 2]);

            ARJourneySessionHelper.RemoveFromSession(_session, [1]);

            CollectionAssert.AreEquivalent(new List<int> { 2 }, ARJourneySessionHelper.GetFromSession(_session).ToList());
            Assert.IsNull(ARJourneySessionHelper.GetNameFromSession(_session, 2));
        }

        [TestMethod]
        public void RemoveFromSession_WhenNothingStored_LeavesSelectionEmpty()
        {
            ARJourneySessionHelper.RemoveFromSession(_session, [1]);

            Assert.AreEqual(0, ARJourneySessionHelper.GetFromSession(_session).Count);
        }

        [TestMethod]
        public void ClearAllFromSession_RemovesNames()
        {
            ARJourneySessionHelper.AddToSession(_session, [1]);
            ARJourneySessionHelper.AddNamesToSession(_session, [KeyValuePair.Create(1, "First")]);

            ARJourneySessionHelper.ClearAllFromSession(_session);

            Assert.IsNull(ARJourneySessionHelper.GetNameFromSession(_session, 1));
        }
    }
}
