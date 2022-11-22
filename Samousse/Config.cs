using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Samousse
{
    public class Config
    {
        public string Token { get; set; }
        public ulong[] AllowedGuilds { get; set; }
    }


    public class ConfigLoader
    {
        public static string ConfFolder = "Data";
        public static string ConfFile = "config.json";
        public static string ConfPath = Path.Combine(ConfFolder, ConfFile);

        private Config _config;

        public Config Config => _config;

        // if _config is null, the app should stop and not continue
        public ConfigLoader()
        {
            Directory.CreateDirectory(ConfFolder);
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (_config == null)
            {
                string path = ConfPath;
                try
                {
                    if (File.Exists(path))
                    {
                        Config? config = JsonSerializer.Deserialize<Config>(File.ReadAllText(path), Utils.JsonSerializerOptionsIndented);
                        if (config != null)
                        {
                            _config = config;
                        }
                        else
                        {
                            Log.Error("Failed to read Config file");
                        }
                    }
                    else
                    {
                        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(new Config(), Utils.JsonSerializerOptionsIndented));
                        Log.Error("Please fill config.json before starting the app");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error when loading config.json {ex.Message}");
                }

                // if conf is null, there should be an error
                if (_config == null)
                {
                    Environment.Exit(-1);
                }
            }
        }
    }
}
