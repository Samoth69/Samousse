using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using System.Collections.Concurrent;

namespace Samousse.Modules.Power4
{
    public class PowerFourModule : InteractionModuleBase<SocketInteractionContext>, IDisposable
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

        public void Dispose()
        {
            _client.MessageReceived -= HandleMessage;
            _client.ThreadDeleted -= HandleThreadDeleted;
            GC.SuppressFinalize(this); //Hey, GC: don't bother calling finalize later
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

            if (YellowPlayer.Id == RedPlayer.Id)
            {
                await RespondAsync("Error: Players must be different");
                return;
            }

            if (Context.Channel is SocketTextChannel stc && stc.GetChannelType() == ChannelType.Text)
            {
                await RespondAsync("Ok");
                var (threadID, engine) = await PowerFourGameEngine.BuildPowerFourGE(stc, YellowPlayer, RedPlayer, (id, _) =>
                {
                    //_engines.Remove(id);
                });
                _engines.Add(threadID, engine);
                await engine.SendStartMessages();
            }
            else
            {
                await RespondAsync("Error: Invalid channel type, make sure you are in a server text channel");
            }

            // delete game that are older than 1 hour
            await Task.Run(async () =>
            {
                var currentTime = DateTime.Now;
                foreach (var item in _engines)
                {
                    // limit game time to 1 hour
                    if (item.Value.StartTime < currentTime - TimeSpan.FromMinutes(60))
                    {
                        _engines.Remove(item.Key, out var deletedObject);
                        if (deletedObject != null)
                        {
                            try
                            {
                                await deletedObject.DestroyChannel();
                            }
                            catch (Exception e)
                            {
                                Log.Error("Failed to delete channel {channelID}: {error}", deletedObject.ChannelId, e);
                            }
                        }
                    }
                }
            });
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
