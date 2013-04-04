using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace AsyncCache.Specs.CacherSpecs.Settings
{
    [TestClass]
    public class IsConfigurable
    {
        [TestMethod]
        public void DefaultSettings()
        {
            new CacheSettings().MaxTimeInCache.Should().Be(TimeSpan.FromHours(1));
        }

        [TestMethod]
        public void CanSetMaxTimeInCache()
        {
            //Arrange

            //Act
            Cacher.Settings.MaxTimeInCache = TimeSpan.FromMilliseconds(200);

            //Assert
            Cacher.Settings.MaxTimeInCache.Should().Be(TimeSpan.FromMilliseconds(200));

            // Reset to defaults
            Cacher.Settings = new CacheSettings();
        }
    }
}
