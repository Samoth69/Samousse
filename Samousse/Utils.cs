using Discord;
using Serilog.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Samousse
{
    public static class Utils
    {
        public readonly static JsonSerializerOptions JsonSerializerOptionsIndented = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public static Task DiscordLog(LogMessage msg)
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
