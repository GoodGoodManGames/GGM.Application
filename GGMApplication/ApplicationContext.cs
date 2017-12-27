using GGM.Context;
using GGM.Context.Attribute;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Reflection.Emit;

using static System.Reflection.Emit.OpCodes;
using GGM.Application.Attribute;

namespace GGM.Application
{
    public sealed class ApplicationContext : ManagedContext
    {
        private delegate void ConfigMapper(object target);

        public ApplicationContext(Assembly assembly, Dictionary<string, string> configs)
        {
            Assembly = assembly;
            mConfigs = configs;

            var allTypes = Assembly.GetTypes();
            var managedTypes = allTypes
                .Where(type => type.IsDefined(typeof(ManagedAttribute)))
                .ToDictionary(type => type, type => type.GetCustomAttribute<ManagedAttribute>());

            // Singleton들은 미리 생성.
            foreach (var managedType in managedTypes)
            {
                // Key : type
                // Value : ManagedAttribute
                if (managedType.Value.ManagedType == ManagedType.Singleton)
                    GetManaged(managedType.Key);
            }
        }

        public Assembly Assembly { get; }
        private Dictionary<string, string> mConfigs;
        private Dictionary<Type, ConfigMapper> mConfigMapperCache = new Dictionary<Type, ConfigMapper>();

        public override object Create(Type type, object[] parameters = null)
        {
            var result = base.Create(type, parameters);

            //TODO: 해당 type에 ConfigAttribute가 없는 경우 아래 일련의 동작을 하지 않으면 속도 항상을 얻을 수 있다.
            if (!mConfigMapperCache.ContainsKey(type))
                mConfigMapperCache[type] = GenerateConfigMapper(type);

            var configMapper = mConfigMapperCache[type];
            configMapper(result);
            return result;
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
                if (configInfo.PropertyInfo.PropertyType != typeof(string))
                    throw new System.Exception("현재 string 타입의 config value만 지원합니다.");

                var key = configInfo.ConfigAttribute.Key;
                if (!mConfigs.ContainsKey(key))
                {
                    Console.WriteLine("해당 Key의 설정이 존재하지 않습니다. 이는 무시됩니다.");
                    continue;
                }

                //TODO: 테스트용 문자열만 박아놓는 코드이다.
                var value = mConfigs[key];
                il.Emit(Ldarg_0); // [targetObject]
                il.Emit(Ldstr, value); // [targetObject] [Value]
                il.Emit(Callvirt, configInfo.PropertyInfo.GetSetMethod()); // Empty
            }
            il.Emit(Ret);

            return dynamicMethod.CreateDelegate(typeof(ConfigMapper)) as ConfigMapper;
        }
    }
}
