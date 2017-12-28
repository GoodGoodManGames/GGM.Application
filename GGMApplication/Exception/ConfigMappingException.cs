using System;
using System.Collections.Generic;
using System.Text;

namespace GGM.Application.Exception
{
    public enum ConfigMappingError
    {
        NotSupportedType
    }

    public class ConfigMappingException : System.Exception
    {
        public ConfigMappingException(ConfigMappingError configMappingError)
            : base($"Fail to mapping config. {nameof(ConfigMappingError)} : {configMappingError}")
        {
            ConfigMappingError = configMappingError;
        }

        public ConfigMappingError ConfigMappingError { get; }

        public static void Check(bool condition, ConfigMappingError configMappingError)
        {
            if (condition != true)
                throw new ConfigMappingException(configMappingError);
        }
    }
}
