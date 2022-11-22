using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Samousse.Modules.Reponse
{
    public class ReponseContent
    {
        public string[] answers { get; set; }
    }

    public class ReponseInteraction : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;

        private static ReponseContent _responses;

        public ReponseInteraction(DiscordSocketClient client)
        {
            _client = client;
            _client.MessageReceived += HandleMessage;
        }

        public static async Task LateInit()
        {
            string path = Path.Combine(ConfigLoader.ConfFolder, "reponse.json");
            if (File.Exists(path))
            {
                Log.Debug($"Loading {path}");
                using (var fs = File.OpenRead(path))
                {
                    await JsonSerializer.DeserializeAsync<ReponseContent>(fs, Utils.JsonSerializerOptionsIndented);
                }
            }
            else
            {
                Log.Warning($"No {path} file found, creating one");
                ReponseContent rc = new();
                rc.answers = new string[] { "coucou", "patacaisse" };
                _responses = rc;

                using (var fs = File.OpenWrite(path))
                {
                    await JsonSerializer.SerializeAsync(fs, rc, Utils.JsonSerializerOptionsIndented);
                }
            }
        }

        private Task HandleMessage(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return Task.CompletedTask;

            if (arg.Content.Contains("samousse", StringComparison.CurrentCultureIgnoreCase) || arg.MentionedUsers.Contains(_client.CurrentUser))
            {
                arg.Channel.SendMessageAsync(":thinking:");
            }

            return Task.CompletedTask;
        }
    }
}
