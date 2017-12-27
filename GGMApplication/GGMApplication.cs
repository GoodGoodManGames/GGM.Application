using System;
using System.Reflection;
using System.Linq;
using GGM.Application.Service;
using GGM.Application.Exception;
using GGM.Context.Attribute;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace GGM.Application
{
    public sealed class GGMApplication
    {
        private GGMApplication(string[] args, Type applicationClass, Dictionary<string, string> configs)
        {
            Arguments = args;
            ApplicationAssembly = applicationClass.Assembly;
            Configs = configs;
        }

        //TODO: 추후 객체화 될 예정
        public string[] Arguments { get; }

        public Assembly ApplicationAssembly { get; private set; }
        public ApplicationContext Context { get; private set; }
        public List<IService> Services { get; private set; }
        public Dictionary<string, string> Configs { get; set; }

        public static GGMApplication Run(Type applicationClass, string configPath, string[] args, params Type[] serviceTypes)
        {
            var application = new GGMApplication(args, applicationClass, ParseConfigInternal(configPath));
            application.RunInternal(serviceTypes);
            return application;
        }

        private void RunInternal(Type[] serviceTypes)
        {
            Context = new ApplicationContext(ApplicationAssembly, Configs);
            // service들을 생성하고 주입한 뒤 부팅함.
            BootServicesInternal(serviceTypes);
        }

        private static Dictionary<string, string> ParseConfigInternal(string path)
        {
            var configs = new Dictionary<string, string>();

            // 설정파일이 지정한 경로에 없을 땐 비어있는 Dictionary를 뱉어야 한다.
            if (!File.Exists(path))
            {
                Console.WriteLine("Cannot find ConfigFile. Check config file path.");
                return configs;
            }

            var lines = File.ReadAllLines(path);
            foreach(var line in lines)
            {
                int seperatorIndex = line.IndexOf("=");
                if(seperatorIndex == 0)
                {
                    Console.WriteLine($@"""{line}"" is wrong format. Currently GGMApplication support only Key=Value format.");
                    continue;
                }

                var key = line.Substring(0, seperatorIndex).Trim();
                if (configs.ContainsKey(key))
                    Console.WriteLine($@"""{key}""가 이미 존재합니다. 이는 덮어쓰기됩니다.");
                configs[key] = line.Substring(seperatorIndex + 1).Trim();
            }

            return configs;
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

            var tasks = Services.Select(service => service.Boot(Arguments));
            Task.WaitAll(tasks.ToArray());
        }

        private void Validate(bool condition, RunApplicationError error) { if (!condition) throw new RunApplicationException(error); }
    }
}