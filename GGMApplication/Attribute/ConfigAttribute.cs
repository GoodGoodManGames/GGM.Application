namespace GGM.Application.Attribute
{
    public class ConfigAttribute : System.Attribute
    {
        public ConfigAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}
