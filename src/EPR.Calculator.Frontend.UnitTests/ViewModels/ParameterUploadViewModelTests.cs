namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ParameterUploadViewModelTests
    {
        private ParameterUploadViewModel _testClass;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = fixture.Create<ParameterUploadViewModel>();
        }

        [TestMethod]
        public void CanSetAndGetParamterErrors()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<List<CreateDefaultParameterSettingErrorDto>>();

            // Act
            _testClass.ParamterErrors = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.ParamterErrors);
        }
    }
}