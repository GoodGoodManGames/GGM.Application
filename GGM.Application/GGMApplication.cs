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
    /// <summary>
    ///     프로그램의 정보를 받아 ApplicationContext를 초기화 시켜주고 Service들을 관리하고 Boot 하는 클래스입니다.
    /// </summary>
    public sealed class GGMApplication
    {
        private GGMApplication(string[] args, Type applicationClass, Dictionary<string, string> configs)
        {
            Arguments = args;
            ApplicationAssembly = applicationClass.Assembly;
            Configs = configs;
        }

        /// <summary>
        ///     어플리케이션 실행시 받는 인자값입니다.
        /// </summary>
        //TODO: 추후 객체화 될 예정
        public string[] Arguments { get; }

        /// <summary>
        ///     샐행되는 어플리케이션의 Assembly입니다.
        /// </summary>
        public Assembly ApplicationAssembly { get; private set; }
        
        /// <summary>
        ///     실행한 어플리케이션의 Context입니다.
        /// </summary>
        public ApplicationContext Context { get; private set; }
        
        /// <summary>
        ///     Application에 등록된 Services들입니다.
        /// </summary>
        public List<IService> Services { get; private set; }
        
        /// <summary>
        ///     설정된 Config 파일의 데이터입니다.
        /// </summary>
        public Dictionary<string, string> Configs { get; set; }

        /// <summary>
        ///     GGMApplication의 객체를 만들어 실행합니다.
        /// </summary>
        /// <param name="applicationClass">GGMApplication을 실행하는 클래스의 타입</param>
        /// <param name="configPath">Config 파일의 path</param>
        /// <param name="args">프로그램 인자값</param>
        /// <param name="serviceTypes">실행하는 서비스들의 타입들</param>
        /// <returns>실행된 GGMApplication의 객체</returns>
        public static GGMApplication Run(Type applicationClass, string configPath, string[] args, params Type[] serviceTypes)
        {
            if(applicationClass == null)
                throw new ArgumentNullException(nameof(applicationClass));
            if(configPath == null)
                throw new ArgumentNullException(nameof(configPath));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            if(serviceTypes == null)
                Console.WriteLine("서비스 타입이 존재하지 않습니다. 이는 아무것도 실행하지 않습니다.");
            
            var application = new GGMApplication(args, applicationClass, ConfigUtil.ParseConfigInternal(configPath));
            application.RunInternal(serviceTypes);
            return application;
        }

        private void RunInternal(Type[] serviceTypes)
        {
            Context = new ApplicationContext(ApplicationAssembly, Configs);
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
                RunApplicationException.Check(isServiceClass, RunApplicationError.IsNotServiceClass);

                bool isNotAbstract = !serviceType.IsAbstract;
                RunApplicationException.Check(isNotAbstract, RunApplicationError.IsAbstractClass);

                // AutoWired된 생성자가 없을 시, 기본 생성자를 반환시킴.
                var constructor = serviceType.GetConstructors().FirstOrDefault(info => info.IsDefined(typeof(AutoWiredAttribute)))
                    ?? serviceType.GetConstructor(Type.EmptyTypes);
                bool hasMatchedConstructor = constructor != null;
                RunApplicationException.Check(hasMatchedConstructor, RunApplicationError.NotExistMatchedServiceConstructor);

                var parameters = constructor.GetParameters().Select(info => Context.GetManaged(info.ParameterType));
                IService serviceObject = constructor.Invoke(parameters.ToArray()) as IService;
                Context.ConfigMapping(serviceObject);
                serviceObject.ID = Guid.NewGuid();
                Services.Add(serviceObject);
            }

            var tasks = Services.Select(StartService);
            Task.WaitAll(tasks.ToArray());
        }

        private Task StartService(IService service)
        {
            Console.WriteLine($"{service.GetType()}+{service.ID}... : Boot");
            return service.Boot(Arguments);
        }
    }
}