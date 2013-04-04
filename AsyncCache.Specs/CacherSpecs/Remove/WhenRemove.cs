using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace AsyncCache.Specs.CacherSpecs.Remove
{
    [TestClass]
    public class WhenRemove
    {
        [TestMethod]
        public void CacheEntryIsRemoved()
        {
            //Arrange
            var key = "removeKey";
            Cacher.Get(key, () => "value");

            //Act
            Cacher.Remove(key);

            //Assert
            Cacher.Get(key, () => "newValue").Should().Be("newValue");
        }
    }
}
