using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Samousse
{
    public class Program
    {
        private DiscordSocketClient _client;
        private ServiceProvider _serviceProvider;

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            Init();

            var conf = _serviceProvider.GetService<Config>();

            _client = new DiscordSocketClient();

            _client.Log += DiscordLog;

            await _client.LoginAsync(TokenType.Bot, conf.Token);
            await _client.StartAsync();

            var channel = await _client.GetChannelAsync(883418664777437227);
            if (channel is ITextChannel abc)
            {
                await abc.SendMessageAsync("bou");
            }

            await Task.Delay(-1);
        }

        private void Init()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton(new ConfigLoader().Config);
        }

        private Task DiscordLog(LogMessage msg)
        {
            LogEventLevel level = msg.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Debug => LogEventLevel.Debug,
                LogSeverity.Verbose => LogEventLevel.Debug,
                _ => throw new ArgumentOutOfRangeException(nameof(msg.Severity), $"{msg.Severity} not expected")
            };

            Log.Write(level, "Discord.NET: [{0}] {1}", msg.Source, msg.Message);
            return Task.CompletedTask;
        }
    }
}