using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samousse.Modules.Power4
{
    /// <summary>
    /// Handle one and only one game of power four
    /// If you need to handle multiple parties, each one of those party must have their own PowerFourGameEngine (PowerFourGE for short)
    /// </summary>
    public class PowerFourGameEngine
    {
        private const string _emojiYellow = ":yellow_circle:";
        private const string _emojiRed = ":red_circle:";
        private const string _emojiNone = ":blue_square:";

        private readonly IUser _yellowPlayer;
        private readonly IUser _redPlayer;
        private readonly SocketThreadChannel _channel;
        private readonly EBoardPawn[,] _board;

        /// <summary>
        /// Represent the player that must place the next pawn
        /// </summary>
        private EBoardPawn NextPlayer;

        public enum EBoardPawn : ushort
        {
            None = 0,
            Yellow = 1,
            Red = 2
        }

        /// <summary>
        /// Build PowerFourGameEngine
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="yellowPlayer"></param>
        /// <param name="redPlayer"></param>
        /// <returns></returns>
        async public static Task<KeyValuePair<ulong, PowerFourGameEngine>> BuildPowerFourGE(SocketTextChannel channel, IUser yellowPlayer, IUser redPlayer)
        {
            var threadChannel = await channel.CreateThreadAsync($"Power 4: {yellowPlayer.Username} against {redPlayer.Username}");
            return new(threadChannel.Id, new PowerFourGameEngine(threadChannel, yellowPlayer, redPlayer));
        }

        private PowerFourGameEngine(SocketThreadChannel channel, IUser yellowPlayer, IUser redPlayer)
        {
            _channel = channel;
            _yellowPlayer = yellowPlayer;
            _redPlayer = redPlayer;
            _board = new EBoardPawn[6, 7];

            NextPlayer = Random.Shared.Next(0, 2) == 0 ? EBoardPawn.Yellow : EBoardPawn.Red;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    _board[i, j] = EBoardPawn.None;
                }
            }

            _channel.SendMessageAsync($"Ready to play\n\n" +
                $"How to play: type from 1 to 7 to place your pawn in the corresponding column\n\n" +
                $"It's {(NextPlayer == EBoardPawn.Yellow ? _yellowPlayer.Mention : _redPlayer.Mention)} turn");
        }

        public async Task ReceiveMessage(IUser user, string msg)
        {
            var toPlacePawn = user.Id == _yellowPlayer.Id ? EBoardPawn.Yellow : EBoardPawn.Red;
            if (toPlacePawn != NextPlayer)
            {
                await _channel.SendMessageAsync($"It's not your turn {user.Mention}");
                return;
            }

            if (int.TryParse(msg, out var column))
            {
                // on user side, we are working from 1 to 7, but on our side, we are working from 0 to 6
                column--;

                if (column < 0 || column > 6)
                {
                    await _channel.SendMessageAsync($"Invalid column {user.Mention}");
                    return;
                }

                var row = GetRow(column);
                if (row == -1)
                {
                    await _channel.SendMessageAsync($"Column {column} is full {user.Mention}");
                    return;
                }

                _board[row, column] = toPlacePawn;
                NextPlayer = NextPlayer == EBoardPawn.Yellow ? EBoardPawn.Red : EBoardPawn.Yellow;
                await PrintBoard();
            }
            else
            {
                await _channel.SendMessageAsync($"Invalid column {user.Mention}");
            }
        }

        /// <summary>
        /// Get the row where the pawn should be placed
        /// </summary>
        /// <param name="column">column number (from 0 to 6)</param>
        /// <returns>height of the new pawn (0 is the top, 5 is the bottom) or -1 if full</returns>
        private int GetRow(int column)
        {
            for (int i = 5; i >= 0; i--)
            {
                if (_board[i, column] == EBoardPawn.None)
                {
                    return i;
                }
            }

            return -1;
        }

        public async Task PrintBoard()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    sb.Append(_board[i, j] switch
                    {
                        EBoardPawn.None => _emojiNone,
                        EBoardPawn.Yellow => _emojiYellow,
                        EBoardPawn.Red => _emojiRed,
                        _ => throw new InvalidEnumArgumentException()
                    });
                }
                sb.AppendLine();
            }
            sb.Append(":one::two::three::four::five::six::seven:");
            await _channel.SendMessageAsync(sb.ToString());
        }
    }
}
