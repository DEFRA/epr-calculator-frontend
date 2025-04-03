using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class ParameterRefreshViewModelTests
    {
        private ParameterRefreshViewModel _testClass;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = fixture.Create<ParameterRefreshViewModel>();
        }

        [TestMethod]
        public void CanSetAndGetParameterTemplateValue()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<List<SchemeParameterTemplateValue>>();

            // Act
            _testClass.ParameterTemplateValue = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.ParameterTemplateValue);
        }
    }
}