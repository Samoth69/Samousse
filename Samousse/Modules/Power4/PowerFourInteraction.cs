using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Samousse.Modules.Power4
{
    public class PowerFourModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionHandler _handler;
        private readonly DiscordSocketClient _client;

        /// <summary>
        /// key: ThreadId
        /// value: gameengine for this thread
        /// </summary>
        private readonly Dictionary<ulong, PowerFourGameEngine> _engines;

        public PowerFourModule(InteractionHandler handler, DiscordSocketClient client)
        {
            _engines = new();

            _handler = handler;
            _client = client;

            _client.MessageReceived += HandleMessage;
        }

        ~PowerFourModule()
        {
            _client.MessageReceived -= HandleMessage;
        }

        [SlashCommand("echo", "echo input text")]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("power-four", "Start a new power four game with specified players, starting player is randomly chosen")]
        public async Task PowerFour(IUser YellowPlayer, IUser RedPlayer)
        {
            if (YellowPlayer.IsBot || RedPlayer.IsBot)
            {
                await RespondAsync("Error: Players must be player (not bot)");
                return;
            }

            if (Context.Channel is SocketTextChannel stc && stc.GetChannelType() == ChannelType.Text)
            {
                await RespondAsync("Ok");
                var (threadID, engine) = await PowerFourGameEngine.BuildPowerFourGE(stc, YellowPlayer, RedPlayer);
                _engines.Add(threadID, engine);
            }
            else
            {
                await RespondAsync("Error: Invalid channel type, make sure you are in a server text channel");
            }
        }

        private async Task HandleMessage(SocketMessage msg)
        {
            if (msg.Author.IsBot)
            {
                return;
            }

            PowerFourGameEngine engine;
            if (msg.Channel is SocketThreadChannel stc && _engines.TryGetValue(stc.Id, out engine))
            {
                await engine.ReceiveMessage(msg.Author, msg.Content);
            }
        }
    }
}
