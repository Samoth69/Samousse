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
        // hauteur d'un puissance 4
        public const ushort _p4Height = 6;

        // larteur d'un puissance 4
        public const ushort _p4Width = 7;

        private const string _emojiYellow = ":yellow_circle:";
        private const string _emojiRed = ":red_circle:";
        private const string _emojiNone = ":blue_square:";

        private readonly IUser _yellowPlayer;
        private readonly IUser _redPlayer;
        private readonly SocketThreadChannel _channel;
        private readonly DateTime _startTime;

        /// <summary>
        /// Used when the game is over
        /// first parameter is threadID, second is the winner (1: yellow, 2: red, 3:tie)
        /// </summary>
        private readonly Action<ulong, int> _gameFinishedAction;

        // hauteur puis largeur
        private readonly EBoardPawn[,] _board;

        /// <summary>
        /// Represent the player that must place the next pawn
        /// </summary>
        private EBoardPawn NextPlayer;

        /// <summary>
        /// List of pawn
        /// this is used has a return value in IsBoardFinished
        /// </summary>
        public enum EBoardPawn : ushort
        {
            None = 0,
            Yellow = 1,
            Red = 2
        }

        /// <summary>
        /// Time when the game technically started
        /// </summary>
        public DateTime StartTime { get => _startTime; }

        /// <summary>
        /// Will be true if the game is finished (either tie or one player won)
        /// </summary>
        public bool IsFinished { get; private set; }

        public ulong ChannelId { get => _channel.Id; }

        /// <summary>
        /// Build PowerFourGameEngine
        /// </summary>
        /// <param name="channel">id will be returned in gameFinished action</param>
        /// <param name="yellowPlayer"></param>
        /// <param name="redPlayer"></param>
        /// <param name="gameFinished">ulong: channel id, game winner (1: yellow, 2: red, 3: tie)</param>
        /// <returns></returns>
        async public static Task<KeyValuePair<ulong, PowerFourGameEngine>> BuildPowerFourGE(SocketTextChannel channel, IUser yellowPlayer, IUser redPlayer, Action<ulong, int> gameFinished)
        {
            var threadChannel = await channel.CreateThreadAsync($"Power 4: {yellowPlayer.Username} against {redPlayer.Username}");
            return new(threadChannel.Id, new PowerFourGameEngine(threadChannel, yellowPlayer, redPlayer, gameFinished));
        }

        /// <summary>
        /// see the <see cref="BuildPowerFourGE(SocketTextChannel, IUser, IUser, Action{ulong, int})">builder</see>
        /// </summary>
        private PowerFourGameEngine(SocketThreadChannel channel, IUser yellowPlayer, IUser redPlayer, Action<ulong, int> gameFinished)
        {
            _channel = channel;
            _yellowPlayer = yellowPlayer;
            _redPlayer = redPlayer;
            _board = new EBoardPawn[_p4Height, _p4Width];
            _gameFinishedAction = gameFinished;
            _startTime = DateTime.Now;

            NextPlayer = Random.Shared.Next(0, 2) == 0 ? EBoardPawn.Yellow : EBoardPawn.Red;
            for (int i = 0; i < _p4Height; i++)
            {
                for (int j = 0; j < _p4Width; j++)
                {
                    _board[i, j] = EBoardPawn.None;
                }
            }
        }

        /// <summary>
        /// Should be called once before the player type
        /// </summary>
        public async Task SendStartMessages()
        {
            await _channel.SendMessageAsync($"Ready to play\n\n" +
                $"How to play: type from 1 to 7 to place your pawn in the corresponding column\n\n" +
                $"It's {(NextPlayer == EBoardPawn.Yellow ? _yellowPlayer.Mention : _redPlayer.Mention)} turn");
            await _channel.SendMessageAsync("Note: the game will be deleted after an hour");
            await PrintBoard();
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

                var end_res = IsBoardFinished(_board);
                if (end_res == 1)
                {
                    await _channel.SendMessageAsync($"Game finished, {_yellowPlayer.Mention} won");
                }
                else if (end_res == 2)
                {
                    await _channel.SendMessageAsync($"Game finished, {_redPlayer.Mention} won");
                }
                else if (end_res == 3)
                {
                    await _channel.SendMessageAsync($"Game finished, it's a draw");
                }

                if (end_res != 0)
                {
                    IsFinished = true;
                    _gameFinishedAction(_channel.Id, end_res);
                }
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
            for (int i = _p4Height - 1; i >= 0; i--)
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
            for (int i = 0; i < _p4Height; i++)
            {
                for (int j = 0; j < _p4Width; j++)
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

        /// <summary>
        /// Check if a player has won
        /// </summary>
        /// <param name="board">board to check</param>
        /// <returns>
        /// -1: tie
        /// 0: game isn't finished
        /// 1: yellow has won
        /// 2: red has won
        /// </returns>
        public static int IsBoardFinished(EBoardPawn[,] board)
        {
            // check horizontal
            for (int i = 0; i < _p4Height; i++)
            {
                for (int j = 0; j < _p4Width - 3; j++)
                {
                    if (board[i, j] != EBoardPawn.None &&
                        board[i, j] == board[i, j + 1] &&
                        board[i, j] == board[i, j + 2] &&
                        board[i, j] == board[i, j + 3])
                    {
                        return (int)board[i, j];
                    }
                }
            }

            // check vertical
            for (int i = 0; i < _p4Height - 3; i++)
            {
                for (int j = 0; j < _p4Width; j++)
                {
                    if (board[i, j] != EBoardPawn.None &&
                        board[i, j] == board[i + 1, j] &&
                        board[i, j] == board[i + 2, j] &&
                        board[i, j] == board[i + 3, j])
                    {
                        return (int)board[i, j];
                    }
                }
            }

            // check diagonal
            for (int i = 0; i < _p4Height - 3; i++)
            {
                for (int j = 0; j < _p4Width - 3; j++)
                {
                    if (board[i, j] != EBoardPawn.None &&
                        board[i, j] == board[i + 1, j + 1] &&
                        board[i, j] == board[i + 2, j + 2] &&
                        board[i, j] == board[i + 3, j + 3])
                    {
                        return (int)board[i, j];
                    }
                }
            }

            // anti diagonal
            for (int i = 0; i < _p4Height - 3; i++)
            {
                for (int j = 3; j < _p4Width; j++)
                {
                    if (board[i, j] != EBoardPawn.None &&
                        board[i, j] == board[i + 1, j - 1] &&
                        board[i, j] == board[i + 2, j - 2] &&
                        board[i, j] == board[i + 3, j - 3])
                    {
                        return (int)board[i, j];
                    }
                }
            }

            // check if there is still a free space
            for (int j = 0; j < _p4Width; j++)
            {
                // we are starting from the top, exiting when
                // there is a pawn.
                for (int i = _p4Height; i >= 0; i--)
                {
                    if (board[i, j] == EBoardPawn.None)
                    {
                        return 0;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Will delete the thread channel
        /// Should be called sometime after the game is done
        /// </summary>
        public async Task DestroyChannel()
        {
            if (_channel != null)
            {
                await _channel.DeleteAsync();
            }
        }
    }
}
