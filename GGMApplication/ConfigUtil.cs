using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GGM.Application
{
    internal static class ConfigUtil
    {
        internal static Dictionary<string, string> ParseConfigInternal(string path)
        {
            var configs = new Dictionary<string, string>();

            // 설정파일이 지정한 경로에 없을 땐 비어있는 Dictionary를 뱉어야 한다.
            if (!File.Exists(path))
            {
                Console.WriteLine("Cannot find ConfigFile. Check config file path.");
                return configs;
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                int seperatorIndex = line.IndexOf("=");
                if (seperatorIndex == 0)
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
    }
}
