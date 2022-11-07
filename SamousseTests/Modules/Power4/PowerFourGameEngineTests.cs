using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samousse.Modules.Power4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Samousse.Modules.Power4.PowerFourGameEngine;

namespace Samousse.Modules.Power4.Tests
{
    [TestClass()]
    public class PowerFourGameEngineTests
    {
        [TestMethod()]
        public void IsBoardFinishedTest()
        {
            EBoardPawn[] pawns = new EBoardPawn[2]
            {
                EBoardPawn.Yellow,
                EBoardPawn.Red
            };
            // execute the same tests for yellow and red pawn
            foreach (var item in pawns)
            {
                TestForColor(item);
            }

            var board1 = GetEmptyBoard();
            var line1 = new EBoardPawn[]
            {
                EBoardPawn.Yellow,
                EBoardPawn.Yellow,
                EBoardPawn.Red,
                EBoardPawn.Red,
                EBoardPawn.Yellow,
                EBoardPawn.Yellow,
                EBoardPawn.Red,
            };
            var line2 = new EBoardPawn[]
            {
                EBoardPawn.Red,
                EBoardPawn.Red,
                EBoardPawn.Yellow,
                EBoardPawn.Yellow,
                EBoardPawn.Red,
                EBoardPawn.Red,
                EBoardPawn.Yellow,
            };

            for (int i = 0; i < _p4Height; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < _p4Width; j++)
                    {
                        board1[i, j] = line1[j];
                    }
                }
                else
                {
                    for (int j = 0; j < _p4Width; j++)
                    {
                        board1[i, j] = line2[j];
                    }
                }
            }

            Assert.AreEqual(-1, IsBoardFinished(board1));

            //removing top piece
            board1[_p4Height - 1, 4] = EBoardPawn.None;

            Assert.AreEqual(0, IsBoardFinished(board1));
        }

        /// <summary>
        /// Return an empty power four board
        /// </summary>
        /// <returns></returns>
        private EBoardPawn[,] GetEmptyBoard()
        {
            var ret = new EBoardPawn[_p4Height, _p4Width];
            for (int i = 0; i < _p4Height; i++)
            {
                for (int j = 0; j < _p4Width; j++)
                {
                    ret[i, j] = EBoardPawn.None;
                }
            }
            return ret;
        }

        /// <summary>
        /// Do a batch of test for a specific pawn (yellow or red)
        /// </summary>
        /// <param name="pawn"></param>
        private void TestForColor(EBoardPawn pawn)
        {
            EBoardPawn[,] board1 = GetEmptyBoard();
            board1[0, 0] = pawn;
            board1[0, 1] = pawn;
            board1[0, 2] = pawn;
            board1[0, 3] = pawn;

            Assert.AreEqual((ushort)pawn, IsBoardFinished(board1));

            board1 = GetEmptyBoard();
            board1[0, 0] = pawn;
            board1[1, 0] = pawn;
            board1[2, 0] = pawn;
            board1[3, 0] = pawn;

            Assert.AreEqual((ushort)pawn, IsBoardFinished(board1));

            board1 = GetEmptyBoard();
            board1[0, 0] = pawn;
            board1[1, 1] = pawn;
            board1[2, 2] = pawn;
            board1[3, 3] = pawn;

            Assert.AreEqual((ushort)pawn, IsBoardFinished(board1));

            board1 = GetEmptyBoard();
            board1[3, 0] = pawn;
            board1[2, 1] = pawn;
            board1[1, 2] = pawn;
            board1[0, 3] = pawn;

            Assert.AreEqual((ushort)pawn, IsBoardFinished(board1));
        }

    }
}
