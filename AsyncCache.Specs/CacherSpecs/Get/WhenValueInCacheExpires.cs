using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NSubstitute;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Caching;

namespace AsyncCache.Specs.CacherSpecs.Get
{
    [TestClass]
    public class WhenValueInCacheExpires
    {
        const string key = "expired";

        TaskCompletionSource<string> taskCompletion;

        [TestInitialize]
        public void TestSetup()
        {
            taskCompletion = new TaskCompletionSource<string>();

            // Make the Clock a static time to aid in time based tests
            Clock.UtcNow = () => DateTime.UtcNow;

            Cacher.Remove(key);

            //Set cache value
            Cacher.Get(key, () => key, TimeSpan.FromMilliseconds(10));

            // Make it expired
            Clock.UtcNow = () => DateTime.UtcNow.AddMinutes(1);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //Cacher.Remove(key);
            MemoryCache.Default.Trim(100); // Remove 100% of the cached entries;
            Clock.UtcNow = () => DateTime.UtcNow;
        }

        [TestMethod]
        public void RefreshesValueOnNextCall()
        {
            //Arrange
            var fake = Substitute.For<ITestable>();
            fake.LongRunningProcess(key).Returns(x => taskCompletion.Task.Result);

            //Act
            string value = Cacher.Get(key, () => fake.LongRunningProcess(key));

            //Assert
            taskCompletion.Task.Wait(10);

            fake.Received(1).LongRunningProcess(key);
        }

        [TestMethod]
        public void ReturnsCurrentlyCachedValue()
        {
            //Arrange

            //Act
            string value = Cacher.Get(key, () => taskCompletion.Task.Result);

            //Assert
            value.Should().Be(key);
        }

        [TestMethod]
        public void RefreshesValueInBackgroundThread()
        {
            //Arrange

            //Act
            string value = Cacher.Get(key, () => taskCompletion.Task.Result);

            //Assert
            value.Should().Be(key);

            taskCompletion.SetResult("newValue");
        }

        [TestMethod]
        public void RefreshValueInMultiThreadApp()
        {
            //Arrange
            var fake = Substitute.For<ITestable>();
            fake.LongRunningProcess(key).Returns(x => taskCompletion.Task.Result);

            Clock.UtcNow = () => DateTime.UtcNow;
            Cacher.Remove(key);
            Cacher.Get(key, () => "orig", TimeSpan.FromMilliseconds(1));

            Clock.UtcNow = () => DateTime.UtcNow.AddMinutes(1);

            // Act
            var task1 = Task.Factory.StartNew(() => Cacher.Get(key, () => fake.LongRunningProcess(key)));
            var task2 = Task.Factory.StartNew(() => Cacher.Get(key, () => fake.LongRunningProcess(key)));

            Task.WaitAll(task1, task2);

            // Assert
            taskCompletion.SetResult("newValue");

            // it should not try to update the expired values more than once
            fake.Received(1).LongRunningProcess(key);
        }
    }
}
