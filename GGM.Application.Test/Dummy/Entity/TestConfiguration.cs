using GGM.Context;
using GGM.Context.Attribute;

namespace GGMApplicationTest.Dummy.Configuration
{
    [Configuration]
    public class TestConfiguration
    {
        [Managed(ManagedType.Proto)]
        public Person GetPerson()
        {
            return new Person{Name = "213", Age = 123};
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}