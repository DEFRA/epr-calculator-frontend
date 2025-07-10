namespace EPR.Calculator.Frontend.UnitTests.Extensions
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SessionExtensionsTests
    {
        [TestMethod]
        public void CanCallSetObject()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var session = fixture.Create<ISession>();
            var key = fixture.Create<string>();
            var value = fixture.Create<string>();

            // Act
            session.SetObject<string>(key, value);

            // Assert
            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void CannotCallGetBooleanFlagWithNullSession()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            Assert.ThrowsException<ArgumentNullException>(() => default(ISession).GetBooleanFlag(fixture.Create<string>()));
        }

        [TestMethod]
        public void CannotCallSetBooleanFlagWithNullSession()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            Assert.ThrowsException<ArgumentNullException>(() => default(ISession).SetBooleanFlag(fixture.Create<string>(), fixture.Create<bool>()));
        }

        [TestMethod]
        public void CannotCallRemoveKeyIfExistsWithNullSession()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            Assert.ThrowsException<ArgumentNullException>(() => default(ISession).RemoveKeyIfExists(fixture.Create<string>()));
        }
    }
}