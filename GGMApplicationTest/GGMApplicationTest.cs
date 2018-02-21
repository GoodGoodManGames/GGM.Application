using GGM.Application;
using GGMApplicationTest.Dummy.Configuration;
using GGMApplicationTest.Dummy.Service;
using Xunit;
using Xunit.Abstractions;

namespace GGMApplicationTest
{
    public class GGMApplicationTest
    {
        public GGMApplicationTest()
        {
            _application = GGMApplication.Run(
                GetType()
                , "../../../application.cfg"
                , new string[] { }
                , typeof(ConstructorService)
                , typeof(NonConstructorService)
            );
        }

        GGMApplication _application;

        [Fact]
        public void ServiceTest()
        {
            Assert.NotNull(_application);
            Assert.NotEmpty(_application.Services);
            Assert.Equal(_application.Services.Count, 2);
            Assert.True(_application.Services[0] is ConstructorService);
            Assert.True(_application.Services[1] is NonConstructorService);
        }

        
        ////// application.cfg's body/////////
        // project.name=GGMApplication Test //
        // project.testint = 100000         //
        // project.testlong = 10000000000   //
        // project.testfloat = 3.3          //
        // project.testdouble = 3.31232451  //
        //////////////////////////////////////
        [Fact]
        public void ConfigMappingTest()
        {
            var service = _application.Services[0] as ConstructorService;
            Assert.Equal(service.ApplicationName, "GGMApplication Test");
            Assert.Equal(service.TestInt, 100000);
            Assert.Equal(service.TestLong, 10000000000);
            Assert.Equal(service.TestFloat, 3.3f);
            Assert.Equal(service.TestDouble, 3.31232451d);
        }
        
        [Fact]
        public void LoadConfigurationTest()
        {
            var configuration = _application.Context.GetManaged<TestConfiguration>();
            Assert.NotNull(configuration);
        }
    }
}