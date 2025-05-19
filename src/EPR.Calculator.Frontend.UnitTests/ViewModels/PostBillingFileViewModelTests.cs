namespace EPR.Calculator.Frontend.UnitTests.ViewModels;

using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PostBillingFileViewModelTests
{
    private PostBillingFileViewModel _testClass;

    [TestInitialize]
    public void SetUp()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        _testClass = fixture.Create<PostBillingFileViewModel>();
    }

    [TestMethod]
    public void ImplementsIEquatable_PostBillingFileViewModel()
    {
        // Arrange
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        var same = new PostBillingFileViewModel();
        var different = fixture.Create<PostBillingFileViewModel>();

        // Assert
        Assert.IsFalse(_testClass.Equals(default(object)));
        Assert.IsFalse(_testClass.Equals(new object()));
        Assert.IsFalse(_testClass.Equals((object)different));
        Assert.IsFalse(_testClass.Equals(different));
        Assert.AreNotEqual(different.GetHashCode(), _testClass.GetHashCode());
        Assert.IsFalse(_testClass == different);
        Assert.IsTrue(_testClass != different);
    }

    [TestMethod]
    public void CanSetAndGetCalculatorRunStatus()
    {
        // Arrange
        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var testValue = fixture.Create<CalculatorRunPostBillingFileDto>();

        // Act
        _testClass.CalculatorRunStatus = testValue;

        // Assert
        Assert.AreSame(testValue, _testClass.CalculatorRunStatus);
    }

    [TestMethod]
    public void CanSetAndGetDownloadResultURL()
    {
        // Arrange
        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var testValue = fixture.Create<Uri>();

        // Act
        _testClass.DownloadResultURL = testValue;

        // Assert
        Assert.AreSame(testValue, _testClass.DownloadResultURL);
    }

    [TestMethod]
    public void CanSetAndGetDownloadErrorURL()
    {
        // Arrange
        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var testValue = fixture.Create<string>();

        // Act
        _testClass.DownloadErrorURL = testValue;

        // Assert
        Assert.AreEqual(testValue, _testClass.DownloadErrorURL);
    }

    [TestMethod]
    public void CanSetAndGetDownloadTimeout()
    {
        // Arrange
        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var testValue = fixture.Create<int?>();

        // Act
        _testClass.DownloadTimeout = testValue;

        // Assert
        Assert.AreEqual(testValue, _testClass.DownloadTimeout);
    }
}