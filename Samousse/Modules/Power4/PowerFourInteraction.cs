using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace Samousse.Modules.Power4
{
    public class PowerFourModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionHandler _handler;
        private readonly DiscordSocketClient _client;
        private readonly bool _isDev;

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
            _client.ThreadDeleted += HandleThreadDeleted;

#if DEBUG
            _isDev = true;
#else
            _isDev = false;
#endif
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
        [SlashCommand("p4", "Start a new power four game with specified players, starting player is randomly chosen")]
        public async Task PowerFour(IUser YellowPlayer, IUser RedPlayer)
        {
            if (!_isDev && (YellowPlayer.IsBot || RedPlayer.IsBot))
            {
                await RespondAsync("Error: Players must be player (not bot)");
                return;
            }

            if (Context.Channel is SocketTextChannel stc && stc.GetChannelType() == ChannelType.Text)
            {
                await RespondAsync("Ok");
                var (threadID, engine) = await PowerFourGameEngine.BuildPowerFourGE(stc, YellowPlayer, RedPlayer, (id, _) =>
                {
                    _engines.Remove(id);
                });
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

        private Task HandleThreadDeleted(Cacheable<SocketThreadChannel, ulong> arg)
        {
            if (_engines.ContainsKey(arg.Value.Id))
            {
                Log.Debug($"Thread {arg.Value.Id} was deleted, removing from memory");
                _engines.Remove(arg.Value.Id);
            }
            return Task.CompletedTask;
        }
    }
}
