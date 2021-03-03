using System.IO;
using System.Text.Json;

namespace DiscordNetTemplate.Modules
{
    public struct Config<T> where T: struct, IConfig
    {
        public T Conf;
        
        public void GenerateConfig()
        {
            Conf = new T();
            
            UpdateConfig();
        }

        public void TryLoadConfig()
        {
            if (!File.Exists(Conf.Path))
            {
                GenerateConfig();

                return;
            }

            try
            {
                Conf = JsonSerializer.Deserialize<T>(File.ReadAllText(Conf.Path));
            }

            catch
            {
                GenerateConfig();
            }
        }

        public void UpdateConfig()
        {
            File.WriteAllText(Conf.Path, JsonSerializer.Serialize(Conf));
        }
    }

    public interface IConfig
    {
        public string Path { get; }
    }
}