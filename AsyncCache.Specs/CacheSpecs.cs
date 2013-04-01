using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCache.Specs
{
    [TestClass]
    public class CacheSpecs
    {
        [TestMethod]
        public void ShouldCallCachedRequest()
        {
            //Arrange
            ICachedClass cached = Substitute.For<ICachedClass>();
            cached.GetData("test").Returns("test data");

            //Act
            string result = Cacher.Get(cacheKey: "key", dataProvider: () => cached.GetData("test"), cacheTimeInMinutes: 10) as string;

            //Assert
            result.Should().Be("test data");
        }

        [TestMethod]
        public void ShouldReturnCachedValueForSuccessiveCalls()
        {
            //Arrange
            ICachedClass cached = Substitute.For<ICachedClass>();
            cached.GetData("test").Returns("testing data");

            //Act
            string call1Result = Cacher.Get("cacheKey", () => cached.GetData("test")) as string;
            string call2Result = Cacher.Get("cacheKey", () => cached.GetData("xxx")) as string;

            //Assert
            cached.Received(1).GetData(Arg.Any<string>());
        }

        [TestMethod]
        public void UseCase_Example()
        {
            //Arrange
            LongProcessClass cached = new LongProcessClass();

            Func<DateTime> originalUtcNow = Clock.UtcNow;
            Clock.UtcNow = () => new DateTime(2013, 1, 1, 0, 0, 0); // 1/1/2013 0:0:0
            string theKey = "theKey";

            //Act
            // Initial action loads the data syncronously
            Task<string> call1Result =
                Task.Factory.StartNew<string>(() =>
                        Cacher.Get(theKey, () => cached.GetViaLongProcess("1"), 10) as string
                );
            // Now getDataWill change its result
            Task<string> call2Result =
                Task.Factory.StartNew<string>(() =>
                    Cacher.Get(theKey, () => cached.GetViaLongProcess("2"), 10) as string
                    );
            // Make sure we have the cached value
            call2Result.Result.Should().Be(call1Result.Result, "Call 2 result did not equal Call 1 result");

            // Advance time 
            Clock.UtcNow = () => new DateTime(2013, 1, 1, 0, 11, 0); // 1/1/2013 0:11:0
            // This call will kick off the loading behind the scenes, the value returned will be the cached value
            string call3Result = Cacher.Get(theKey, () => cached.GetViaLongProcess("2"), 10) as string;
            call3Result.Should().Be(call1Result.Result, "Call 3 result did not equal Call 1 result");

            // This call should return the new value from the cache
            //Pause enough time for the cache to get refreshed
            Thread.Sleep(TimeSpan.FromSeconds(6));
            string call4Result = Cacher.Get(theKey, () => cached.GetViaLongProcess("3"), 10) as string;
            call4Result.Should().Be("2", "Call 4 Result did not equal the new value");

            //Assert
            Clock.UtcNow = originalUtcNow;
        }
    }

    public interface ICachedClass
    {
        string GetData(string url);
    }
    public class LongProcessClass
    {
        public string GetViaLongProcess(string valueToEcho)
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
            return valueToEcho;
        }
    }
}
