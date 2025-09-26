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
    public class SchemeParametersViewModelTests
    {
        private SchemeParametersViewModel _testClass = null!;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = fixture.Create<SchemeParametersViewModel>();
        }

        [TestMethod]
        public void CanSetAndGetDefaultSchemeParameters()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<IEnumerable<DefaultSchemeParameters>>();

            // Act
            _testClass.DefaultSchemeParameters = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.DefaultSchemeParameters);
        }

        [TestMethod]
        public void CanSetAndGetSchemeParameterName()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.SchemeParameterName = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.SchemeParameterName);
        }

        [TestMethod]
        public void CanSetAndGetIsDisplayPrefix()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<bool>();

            // Act
            _testClass.IsDisplayPrefix = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.IsDisplayPrefix);
        }
    }
}