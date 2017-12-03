using GGM.Application;
using GGMApplicationTest.Dummy.Service;
using Xunit;
using Xunit.Abstractions;

namespace GGMApplicationTest
{
    public class GGMApplicationTest
    {
        public ITestOutputHelper Output { get; }

        public GGMApplicationTest(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void ServiceTest()
        {
            var application = GGMApplication.Run(this.GetType(), new string[] { }
            , typeof(ConstructorService)
            , typeof(NonConstructorService)
            );
            Assert.NotNull(application);
            Assert.NotEmpty(application.Services);
            Assert.Equal(application.Services.Count, 2);
            Assert.True(application.Services[0] is ConstructorService);
            Assert.True(application.Services[1] is NonConstructorService);
        }

        [Fact]
        public void NonServiceTest()
        {
            var application = GGMApplication.Run(this.GetType(), new string[] { });
            Assert.NotNull(application);
        }
    }
}
