using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class TestTheServicesFixture
    {
        private int setupCallsFinished = 0;

        [Fact]
        public async void CanUse2AtOnce()
        {
            var sf1 = new ServicesFixture();
            var sf2 = new ServicesFixture();
            var whenAll = Task.WhenAll(CallSetupAsync(sf1), CallSetupAsync(sf2));
            setupCallsFinished.ShouldBe(0);
            await whenAll;
            setupCallsFinished.ShouldBe(2);
        }

        private Task CallSetupAsync(ServicesFixture sf)
        {
            var t = Task.Run(() =>
            {
                sf.SetupTraining();
                setupCallsFinished++;
            });
            setupCallsFinished.ShouldBe(0);
            return t;
        }
    }
}