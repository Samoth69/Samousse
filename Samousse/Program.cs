using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Samousse.Modules.Power4;
using Serilog;
using Serilog.Events;
using System.Security.Cryptography.X509Certificates;

namespace Samousse
{
    public class Program
    {
        private readonly ServiceProvider _serviceProvider;

        public Program()
        {
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .CreateLogger();

            _serviceProvider = new ServiceCollection()
                .AddSingleton(new ConfigLoader().Config)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .BuildServiceProvider();
        }

        public static void Main(string[] args) => new Program().RunAsync().GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            var conf = _serviceProvider.GetService<Config>();
            if (conf == null)
            {
                Log.Fatal("Config is null, make sure config file is correct and readable by the bot");
                return;
            }

            var _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            _client.Log += Utils.DiscordLog;

            await _serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, conf.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}