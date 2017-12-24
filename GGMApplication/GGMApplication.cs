using System;
using System.Reflection;
using System.Linq;
using GGM.Application.Service;
using GGM.Application.Exception;
using GGM.Context.Attribute;
using System.Collections.Generic;

namespace GGM.Application
{
    public sealed class GGMApplication
    {
        private GGMApplication(string[] args, Type applicationClass)
        {
            Arguments = args;
            ApplicationAssembly = applicationClass.Assembly;
        }

        //TODO: 추후 객체화 될 예정
        public string[] Arguments { get; }

        public Assembly ApplicationAssembly { get; private set; }
        public ApplicationContext Context { get; private set; }
        public List<IService> Services { get; private set; }

        public static GGMApplication Run(Type applicationClass, string[] args, params Type[] serviceTypes)
        {
            var application = new GGMApplication(args, applicationClass);
            application.RunInternal(serviceTypes);
            return application;
        }

        private void RunInternal(Type[] serviceTypes)
        {
            Context = new ApplicationContext(ApplicationAssembly);
            // service들을 생성하고 주입한 뒤 부팅함.
            BootServicesInternal(serviceTypes);
        }

        private void BootServicesInternal(Type[] serviceTypes)
        {
            if (serviceTypes == null || serviceTypes.Length == 0)
                return;

            Services = new List<IService>(serviceTypes.Length);
            foreach (var serviceType in serviceTypes)
            {
                bool isServiceClass = typeof(IService).IsAssignableFrom(serviceType);
                Validate(isServiceClass, RunApplicationError.IsNotServiceClass);

                bool isNotAbstract = !serviceType.IsAbstract;
                Validate(isNotAbstract, RunApplicationError.IsAbstractClass);

                // AutoWired된 생성자가 없을 시, 기본 생성자를 반환시킴.
                var constructor = serviceType.GetConstructors().FirstOrDefault(info => info.IsDefined(typeof(AutoWiredAttribute)))
                    ?? serviceType.GetConstructor(Type.EmptyTypes);
                bool hasMatchedConstructor = constructor != null;
                Validate(hasMatchedConstructor, RunApplicationError.NotExistMatchedServiceConstructor);

                var parameters = constructor.GetParameters().Select(info => Context.GetManaged(info.ParameterType));
                IService serviceObject = constructor.Invoke(parameters.ToArray()) as IService;
                serviceObject.ID = Guid.NewGuid();
                Services.Add(serviceObject);
            }

            foreach (var service in Services)
                service.Boot(Arguments);
        }

        private void Validate(bool condition, RunApplicationError error) { if (!condition) throw new RunApplicationException(error); }
    }
}