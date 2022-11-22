using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samousse.Modules.Reponse
{
    public class ReponseInteraction : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;

        public ReponseInteraction(DiscordSocketClient client)
        {
            _client = client;
            _client.MessageReceived += HandleMessage;
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
