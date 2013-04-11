using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;

namespace AsyncCache.Specs.CacherSpecs.Get
{
    [TestClass]
    public class WhenDelegateThrowsException
    {
        private const string key = "exception";

        [TestInitialize]
        public void TestSetup()
        {
            Cacher.Remove(key);
        }

        [TestMethod]
        public void FirstCallThrowsException()
        {
            //Assemble
            //Act
            Action act = () => Cacher.Get<string>(key, () => { throw new Exception("test"); });

            //Assert
            act.ShouldThrow<Exception>().WithMessage("test");
        }

        [TestMethod]
        public void SecondaryCallsKeepsCurrentValue()
        {
            // Assemble
            //Act
            string firstResult = Cacher.Get(key, () => "1");
            string secondResult = Cacher.Get<string>(key, () => { throw new Exception("failed"); });
            string lastResult = Cacher.Get(key, () => "last");

            //Assert
            lastResult.Should().Be(firstResult).And.Be(secondResult);
        }
    }
}
