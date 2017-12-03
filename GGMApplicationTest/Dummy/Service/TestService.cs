using System;
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

        public void Boot(string[] arguments)
        {
            IsAlreadyBoot = true;
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
