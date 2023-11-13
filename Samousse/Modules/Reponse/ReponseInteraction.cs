using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using System.Text.Json;

namespace Samousse.Modules.Reponse
{
    public class ReponseContent
    {
        public bool Enabled { get; set; }
        public string[] Answers { get; set; }

        public ReponseContent()
        {
            Enabled = true;
            Answers = Array.Empty<string>();
        }
    }

    public class ReponseInteraction : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;

        private static ReponseContent _config;

        private readonly Random _random = new Random(Environment.TickCount);

        public ReponseInteraction(DiscordSocketClient client)
        {
            string path = Path.Combine(ConfigLoader.ConfFolder, "reponse.json");
            if (File.Exists(path))
            {
                Log.Debug($"Loading {path}");
                using (var fs = File.OpenRead(path))
                {
                    var res = JsonSerializer.Deserialize<ReponseContent>(fs, Utils.JsonSerializerOptionsIndented);
                    if (res is not null)
                    {
                        _config = res;
                    }
                    else
                    {
                        Log.Error($"Failed to load {path}");
                    }
                }
            }
            else
            {
                Log.Warning($"No {path} file found, creating one");
                ReponseContent rc = new();
                rc.Enabled = false;
                rc.Answers = new string[] { "coucou", "patacaisse" };
                _config = rc;

                using (var fs = File.OpenWrite(path))
                {
                    JsonSerializer.Serialize(fs, rc, Utils.JsonSerializerOptionsIndented);
                }
            }

            if (_config is null)
            {
                Log.Fatal("ReponseContent is null, make sure config file is correct and readable by the bot");
                return;
            }

            _client = client;
            _client.MessageReceived += HandleMessage;
        }

        private Task HandleMessage(SocketMessage arg)
        {
            if (arg.Author.IsBot || !_config.Enabled)
                return Task.CompletedTask;

            if (arg.Content.Contains("samousse", StringComparison.CurrentCultureIgnoreCase) || arg.MentionedUsers.Where(x => x.Id == _client.CurrentUser.Id).Any())
            {
                var i = _random.Next(0, _config.Answers.Length);
                arg.Channel.SendMessageAsync(_config.Answers[i]);
            }

            return Task.CompletedTask;
        }
    }
}
