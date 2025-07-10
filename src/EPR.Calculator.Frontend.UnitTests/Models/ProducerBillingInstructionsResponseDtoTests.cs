namespace EPR.Calculator.Frontend.UnitTests.Models
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProducerBillingInstructionsResponseDtoTests
    {
        private ProducerBillingInstructionsResponseDto _testClass;
        private List<ProducerBillingInstructionsDto> _records;
        private int _totalRecords;
        private int _calculatorRunId;
        private string _runName;
        private int? _pageNumber;
        private int? _pageSize;
        private IEnumerable<int> _allProducerIds;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _records = fixture.Create<List<ProducerBillingInstructionsDto>>();
            _totalRecords = fixture.Create<int>();
            _calculatorRunId = fixture.Create<int>();
            _runName = fixture.Create<string>();
            _pageNumber = fixture.Create<int?>();
            _pageSize = fixture.Create<int?>();
            _allProducerIds = fixture.Create<IEnumerable<int>>();
            _testClass = fixture.Create<ProducerBillingInstructionsResponseDto>();
        }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new ProducerBillingInstructionsResponseDto
            {
                Records = _records,
                TotalRecords = _totalRecords,
                CalculatorRunId = _calculatorRunId,
                RunName = _runName,
                PageNumber = _pageNumber,
                PageSize = _pageSize,
                AllProducerIds = _allProducerIds
            };

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}