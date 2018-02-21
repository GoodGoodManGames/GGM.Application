using GGM.Context;
using GGM.Context.Attribute;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Reflection.Emit;

using static System.Reflection.Emit.OpCodes;
using GGM.Application.Attribute;
using System.Runtime.CompilerServices;
using GGM.Application.Exception;

namespace GGM.Application
{
    /// <summary>
    ///     어플리케이션 전체의 Context입니다. 적용되는 Assembly의 Singleton Managed 클래스들을 미리 로드합니다.
    ///     주입될때 생성되는 Managed들의 ConfigAttribute가 달린 프로퍼티들에 값을 매핑해줍니다. 
    /// </summary>
    public sealed class ApplicationContext : ManagedContext
    {
        private delegate void ConfigMapper(object target);

        /// <param name="assembly">Scan할 Assembly</param>
        /// <param name="configs">설정 데이터</param>
        public ApplicationContext(Assembly assembly, Dictionary<string, string> configs)
        {
            Assembly = assembly;
            _configs = configs;
            
            // 현재 어셈블리의 Singleton들은 미리 생성.
            var currentAssemblySignletonTypes = assembly.GetTypes()
                .Where(type => type.IsDefined(typeof(ManagedAttribute)))
                .ToDictionary(type => type, type => type.GetCustomAttribute<ManagedAttribute>());
            foreach (var managedType in currentAssemblySignletonTypes)
            {
                if (managedType.Value.ManagedType == ManagedType.Singleton)
                    GetManaged(managedType.Key);
            }
        }
        
        static private IEnumerable<Type> Explore(Assembly assembly)
        {
            return assembly.GetTypes().SelectMany(ApplicationContext.Explore);
        }

        static private IEnumerable<Type> Explore(Type type)
        {
            foreach (var _type in type.GetNestedTypes(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly).SelectMany(ApplicationContext.Explore)) { yield return _type; }
            yield return type;
        }

        static private IEnumerable<Type> Explore()
        {
            var _domain = AppDomain.CurrentDomain.GetAssemblies();
            return _domain.SelectMany(ApplicationContext.Explore);
        }

        /// <summary>
        ///     Scan한 Assembly
        /// </summary>
        public Assembly Assembly { get; }
        private Dictionary<string, string> _configs;
        private Dictionary<Type, ConfigMapper> _configMapperCache = new Dictionary<Type, ConfigMapper>();

        public override object Create(BaseManagedDefinition definition, object[] parameters = null)
        {
            var result = base.Create(definition, parameters);

            //TODO: 해당 type에 ConfigAttribute가 없는 경우 아래 일련의 동작을 하지 않으면 속도 항상을 얻을 수 있다.
            //      Singleton인 경우는 성능 누락이 의미없겠지만, Proto의 경우에는 다름.
            ConfigMapping(result);

            return result;
        }

        // TODO: 이는 추후 통합되어야 합니다.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ConfigMapping(object target)
        {
            var type = target.GetType();
            if (!_configMapperCache.ContainsKey(type))
                _configMapperCache[type] = GenerateConfigMapper(type);

            var configMapper = _configMapperCache[type];
            configMapper(target);
        }

        private ConfigMapper GenerateConfigMapper(Type type)
        {
            var configInfos = type.GetProperties()
                                    .Select(info => (PropertyInfo: info, ConfigAttribute: info.GetCustomAttribute<ConfigAttribute>()))
                                    .Where(info => info.ConfigAttribute != null);
            var dynamicMethod = new DynamicMethod($"{type}ConfigMapper+{Guid.NewGuid()}", null, new[] { typeof(object) });
            var il = dynamicMethod.GetILGenerator();

            foreach (var configInfo in configInfos)
            {
                var key = configInfo.ConfigAttribute.Key;
                if (!_configs.ContainsKey(key))
                {
                    Console.WriteLine("해당 Key의 설정이 존재하지 않습니다. 이는 무시됩니다.");
                    continue;
                }

                var propertyInfo = configInfo.PropertyInfo;
                var propertyType = propertyInfo.PropertyType;

                var value = _configs[key];
                il.Emit(Ldarg_0); // [targetObject]
                #region [targetObject] [Value]
                if (propertyInfo.PropertyType == typeof(string))
                    il.Emit(Ldstr, value);
                else if (propertyType == typeof(int))
                    il.Emit(Ldc_I4, int.Parse(value));
                else if (propertyType == typeof(long))
                    il.Emit(Ldc_I8, long.Parse(value));
                else if (propertyType == typeof(float))
                    il.Emit(Ldc_R4, float.Parse(value));
                else if (propertyType == typeof(double))
                    il.Emit(Ldc_R8, double.Parse(value));
                else
                    throw new ConfigMappingException(ConfigMappingError.NotSupportedType);
                #endregion
                il.Emit(Callvirt, propertyInfo.GetSetMethod()); // Empty
            }
            il.Emit(Ret);

            return dynamicMethod.CreateDelegate(typeof(ConfigMapper)) as ConfigMapper;
        }
    }
}
