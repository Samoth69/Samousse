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
        //public ReponseConfig Reponse { get; set; }

        //public class ReponseConfig
        //{
        //    /// <summary>
        //    /// True if we should enable this module
        //    /// </summary>
        //    public bool Enable { get; set; }

        //    /// <summary>
        //    /// Delay between two answers
        //    /// </summary>
        //    public int DelayBetweenAnswers { get; set; }

        //    public ContextAnswers[]? Answers { get; set; }
            
        //    /// <summary>
        //    /// Contient une réponse possible pour le message donné
        //    /// </summary>
        //    public class ContextAnswers
        //    {
        //        /// <summary>
        //        /// Regex à match pour savoir quoi répondre
        //        /// </summary>
        //        public string? Regex { get; set; }
        //        public string[]? Answers { get; set; }
        //        public string[]? Reactions { get; set; }
        //    }
        //}
    }

    public class ConfigLoader
    {
        private static string _configFileName = $"data{Path.DirectorySeparatorChar}config.json";
        private Config _config;

        public Config Config => _config;

        // if _config is null, the app should stop and not continue
        public ConfigLoader()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configFileName));
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (_config == null)
            {
                string path = _configFileName;
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
