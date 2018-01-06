using System;
using System.Collections.Generic;
using System.Text;

namespace GGM.Application.Exception
{
    /// <summary>
    ///     ConfigMappingException 에러타입 입니다. 
    /// </summary>
    public enum ConfigMappingError
    {
        /// <summary>
        ///     설정이 매핑되는 타입이 지원되지 않음
        /// </summary>
        NotSupportedType
    }

    /// <summary>
    ///     ApplicationContext에서 설정값을 매핑해 주는데 발생하는 예외입니다.
    /// </summary>
    public class ConfigMappingException : System.Exception
    {
        /// <param name="configMappingError">에러타입</param>
        public ConfigMappingException(ConfigMappingError configMappingError)
            : base($"Fail to mapping config. {nameof(ConfigMappingError)} : {configMappingError}")
        {
            ConfigMappingError = configMappingError;
        }

        /// <summary>
        ///     에러타입
        /// </summary>
        public ConfigMappingError ConfigMappingError { get; }

        internal static void Check(bool condition, ConfigMappingError configMappingError)
        {
            if (condition != true)
                throw new ConfigMappingException(configMappingError);
        }
    }
}
