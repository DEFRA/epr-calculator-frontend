using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityDataUtilTests
    {
        [TestMethod]
        public void GetLocalAuthorityData_NullInput_ReturnsNull()
        {
            // Arrange
            var emptyList = new List<LocalAuthorityDisposalCost>();
            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(emptyList, string.Empty);

            // Assert
            Assert.AreEqual(0, result?.Count);
        }

        [TestMethod]
        public void GetLocalAuthorityData_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<LocalAuthorityDisposalCost>();

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(emptyList, string.Empty);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetLocalAuthorityData_SingleItem_ReturnsSingleItem()
        {
            // Arrange
            var singleItemList = new List<LocalAuthorityDisposalCost>
        {
            new LocalAuthorityDisposalCost
            {
                Id = 1, ProjectionYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapTempUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m
            },
        };

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(singleItemList, MaterialTypes.Other);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("England", result[0].Country);
        }

        [TestMethod]
        public void GetLocalAuthorityData_MultipleItemsDifferentCountries_GroupsByCountry()
        {
            // Arrange
            var multipleItems = new List<LocalAuthorityDisposalCost>
        {
            new LocalAuthorityDisposalCost
            {
                Id = 1, ProjectionYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapTempUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m
            },
            new LocalAuthorityDisposalCost
            {
                Id = 9, ProjectionYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapTempUniqueRef = "NI-AL", Country = "Northern Ireland", Material = "Aluminium", TotalCost = 10m
            },
            new LocalAuthorityDisposalCost
            {
                Id = 20, ProjectionYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-PC", Country = "Scotland", Material = "Paper or card", TotalCost = 23.01m
            },
        };

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(multipleItems, MaterialTypes.Other);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("England", result[0].Country);
            Assert.AreEqual("Northern Ireland", result[1].Country);
            Assert.AreEqual("Scotland", result[2].Country);
        }

        [TestMethod]
        public void GetLocalAuthorityData_MultipleItemsSameCountry_GroupsByCountry()
        {
            // Arrange
            var multipleItems = new List<LocalAuthorityDisposalCost>
        {
            new LocalAuthorityDisposalCost
            {
                Id = 1, ProjectionYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapTempUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m
            },
            new LocalAuthorityDisposalCost
            {
                Id = 2, ProjectionYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapTempUniqueRef = "ENG-FC", Country = "England", Material = "Fibre composite", TotalCost = 2210m
            },
        };

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(multipleItems, MaterialTypes.Other);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("England", result[0].Country);
            Assert.AreEqual("England", result[1].Country);
        }

        [TestMethod]
        public void GetLocalAuthorityData_ItemWithSpecificMaterialType_HandlesCorrectly()
        {
            // Arrange
            var items = new List<LocalAuthorityDisposalCost>
        {
            new LocalAuthorityDisposalCost
            {
                Id = 1, ProjectionYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapTempUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m
            },
            new LocalAuthorityDisposalCost
            {
                Id = 2, ProjectionYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapTempUniqueRef = "ENG-FC", Country = "England", Material = "Other", TotalCost = 2210m
            },
        };

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(items, MaterialTypes.Other);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("England", result[1].Country);
            Assert.AreEqual("Test User", result[1].CreatedBy);
            Assert.AreEqual("28 Aug 2024  at 10:12", result[1].CreatedAt);
            Assert.AreEqual(new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), result[1].EffectiveFrom);
            Assert.AreEqual("£2210.00", result[1].TotalCost);
            Assert.AreEqual(MaterialTypes.Other, result[1].Material);
        }
    }
}