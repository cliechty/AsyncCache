using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using NSubstitute;
using System.Threading.Tasks;

namespace AsyncCache.Specs.CacherSpecs.Get
{
    [TestClass]
    public class WhenValueNotInCache
    {
        const string key = "newKey";

        [TestInitialize]
        public void TestSetup()
        {
            Cacher.Remove(key);
        }

        [TestMethod]
        public void ReturnsFuncResult()
        {
            //Arrange
            //Act
            string value = Cacher.Get(key, () => "value");

            //Assert
            value.Should().Be("value");
        }

        [TestMethod]
        public void OnlyCallsFuncOnce()
        {
            //Arrange
            var fake = Substitute.For<ITestable>();
            fake.LongRunningProcess(key).Returns(key);

            //Act
            Task<string> value1 = Task.Factory.StartNew(() => Cacher.Get(key, () => fake.LongRunningProcess(key)));
            Task<string> value2 = Task.Factory.StartNew(() => Cacher.Get(key, () => fake.LongRunningProcess(key)));

            //Assert
            value1.Wait();
            fake.Received(1).LongRunningProcess(key);
        }

        [TestMethod]
        public void BlocksAllRequestThreads()
        {
            //Arrange
            var tcs = new TaskCompletionSource<string>();

            var fake = Substitute.For<ITestable>();
            fake.LongRunningProcess(key).Returns(x=> tcs.Task.Result);

            //Act
            Task<string> value1 = Task.Factory.StartNew(() => Cacher.Get(key, () => fake.LongRunningProcess(key)));
            Task<string> value2 = Task.Factory.StartNew(() => Cacher.Get(key, () => fake.LongRunningProcess(key)));

            //Assert
            value1.IsCompleted.Should().BeFalse("First Thread is not blocked");
            value2.IsCompleted.Should().BeFalse("Second Thread is not blocked");

            // Complete the long running process
            tcs.SetResult(key);

            value1.Result.Should().Be(key, "Value1 did not match key");
            value2.Result.Should().Be(key, "Value2 did not match key");
        }
    }
}
