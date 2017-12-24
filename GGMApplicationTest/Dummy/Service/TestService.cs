using System;
using System.Threading.Tasks;
using GGM.Application.Service;
using GGM.Context.Attribute;
using UnitTest.Dummy.Entity;

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
