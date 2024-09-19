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
            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetLocalAuthorityData_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<LocalAuthorityDisposalCost>();

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(emptyList);

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
                Id = 1, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapDataTemplateMasterUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m
            },
        };

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(singleItemList);

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
                Id = 1, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapDataTemplateMasterUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m
            },
            new LocalAuthorityDisposalCost
            {
                Id = 9, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapDataTemplateMasterUniqueRef = "NI-AL", Country = "NI", Material = "Aluminium", TotalCost = 10m
            },
            new LocalAuthorityDisposalCost
            {
                Id = 20, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-PC", Country = "Scotland", Material = "Paper or card", TotalCost = 23.01m
            },
        };

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(multipleItems);

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
                Id = 1, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapDataTemplateMasterUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m
            },
            new LocalAuthorityDisposalCost
            {
                Id = 2, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapDataTemplateMasterUniqueRef = "ENG-FC", Country = "England", Material = "Fibre composite", TotalCost = 2210m
            },
        };

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(multipleItems);

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
                Id = 1, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapDataTemplateMasterUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m
            },
            new LocalAuthorityDisposalCost
            {
                Id = 2, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1,
                LapcapDataTemplateMasterUniqueRef = "ENG-FC", Country = "England", Material = "Other", TotalCost = 2210m
            },
        };

            // Act
            var result = LocalAuthorityDataUtil.GetLocalAuthorityData(items);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(MaterialTypes.Other, result[1].Material);
        }
    }
}