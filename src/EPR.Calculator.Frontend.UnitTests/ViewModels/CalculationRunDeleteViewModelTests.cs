using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class CalculationRunDeleteViewModelTests
    {
        public CalculationRunDeleteViewModelTests()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
        }

        private IFixture Fixture { get; init; }

        [TestMethod]
        public void CanSetAndGetCalculatorRunStatusDataValue()
        {
            // Arrange
            var testValue = Fixture.Create<CalculatorRunStatusUpdateDto>();
            var sut = Fixture.Create<CalculationRunDeleteViewModel>() with { CalculatorRunStatusData = testValue };

            // Assert
            Assert.AreSame(testValue, sut.CalculatorRunStatusData);
        }
    }
}
