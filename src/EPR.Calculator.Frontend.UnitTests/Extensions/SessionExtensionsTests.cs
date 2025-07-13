namespace EPR.Calculator.Frontend.UnitTests.Extensions
{
    using System;
    using System.Text;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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

        [TestMethod]
        public void RemoveKeyIfExists_KeyExists_RemovesKey()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var keys = new List<string> { "TestKey" };
            mockSession.Setup(s => s.Keys).Returns(keys);
            mockSession.Setup(s => s.Remove("TestKey"));

            // Act
            mockSession.Object.RemoveKeyIfExists("TestKey");

            // Assert
            mockSession.Verify(s => s.Remove("TestKey"), Times.Once);
        }

        [TestMethod]
        public void RemoveKeyIfExists_KeyDoesNotExist_DoesNotRemoveKey()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var keys = new List<string> { "OtherKey" };
            mockSession.Setup(s => s.Keys).Returns(keys);

            // Act
            mockSession.Object.RemoveKeyIfExists("TestKey");

            // Assert
            mockSession.Verify(s => s.Remove(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void SetBooleanFlag_ShouldStoreBooleanAsString()
        {
            // Arrange
            var sessionMock = new Mock<ISession>();
            var sessionData = new Dictionary<string, byte[]>();

            // Setup mock behavior
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                       .Callback<string, byte[]>((key, value) => sessionData[key] = value);

            var session = sessionMock.Object;
            string testKey = "TestFlag";
            bool testValue = true;

            // Act
            session.SetBooleanFlag(testKey, testValue);

            // Assert
            Assert.IsTrue(sessionData.ContainsKey(testKey));
            string storedValue = Encoding.UTF8.GetString(sessionData[testKey]);
            Assert.AreEqual("True", storedValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetBooleanFlag_ShouldThrowIfSessionIsNull()
        {
            // Act
            ISession nullSession = null;
            nullSession.SetBooleanFlag("SomeKey", true);
        }
    }
}