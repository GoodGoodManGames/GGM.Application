using System;
using System.Threading.Tasks;
using GGM.Application.Service;
using GGM.Context.Attribute;
using UnitTest.Dummy.Entity;
using GGM.Application.Attribute;

namespace GGMApplicationTest.Dummy.Service
{
    public abstract class TestService : IService
    {
        public Guid ID { get; set; }
        public object Managed { get; protected set; }

        public bool IsAlreadyBoot { get; set; }

        public Task Boot(string[] arguments)
        {
            return Task.Run(() => { IsAlreadyBoot = true; });
        }
    }

    public class ConstructorService : TestService
    {
        [Config("project.name")]
        public string ApplicationName { get; set; }
        [Config("project.testint")]
        public int TestInt { get; set; }
        [Config("project.testlong")]
        public long TestLong { get; set; }
        [Config("project.testfloat")]
        public float TestFloat { get; set; }
        [Config("project.testdouble")]
        public double TestDouble { get; set; }
        [AutoWired]
        public ConstructorService(SingletonManaged singletonManaged)
        {
            Managed = singletonManaged;
        }
    }

    public class NonConstructorService : TestService
    {
    }
}
