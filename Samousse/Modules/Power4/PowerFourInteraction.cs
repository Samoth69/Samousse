using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samousse.Modules.Power4
{
    public class PowerFourModule : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionHandler _handler;

        public PowerFourModule(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("echo", "echo input text")]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }
    }
}
