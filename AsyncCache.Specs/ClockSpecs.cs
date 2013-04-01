using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;

namespace AsyncCache.Specs
{
    [TestClass]
    public class ClockSpecs
    {
        private Func<DateTime> originalNow;
        private Func<DateTime> originalUtcNow;

        [TestInitialize]
        public void TestSetup()
        {
            originalNow = Clock.Now;
            originalUtcNow = Clock.UtcNow;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Clock.Now = originalNow;
            Clock.UtcNow = originalUtcNow;
        }

        [TestMethod]
        public void Now_ReturnsCurrentTime()
        {
            //Arrange
            //Act
            DateTime result = Clock.Now();

            //Assert
            result.Should().BeCloseTo(DateTime.Now);
        }

        [TestMethod]
        public void UtcNow_ReturnsCurrentUtcTime()
        {
            //Arrange
            //Act
            DateTime result = Clock.UtcNow();

            //Assert
            result.Should().BeCloseTo(DateTime.UtcNow);
        }

        [TestMethod]
        public void Now_CanBeCustomized()
        {
            //Arrange
            DateTime now = new DateTime(2013, 1, 2);

            //Act
            Clock.Now = () => now;

            //Assert
            Clock.Now().Should().Be(now);
        }

        [TestMethod]
        public void UtcNow_CanBeCustomized()
        {
            //Arrange
            DateTime staticTime = new DateTime(2013, 1, 1);

            //Act
            Clock.UtcNow = () => staticTime;

            //Assert
            Clock.UtcNow().Should().Be(staticTime);
        }
    }
}
