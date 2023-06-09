﻿using Minesolver.CLI;
using Minesolver.Game;

namespace Minesolver.Solver {
    internal class RandomSolver : ISolver {
        public Board Board { get; private set; }

        private readonly Random rand = new Random();

        public RandomSolver(Board board) {
            Board = board;
        }

        public void Solve() {
            float interval;
            while(true) {
                ConsoleHelper.Write("Click interval (seconds): ", ConsoleColor.DarkGray);
                string? strInterval = Console.ReadLine();
                if(strInterval == null) {
                    ConsoleHelper.WriteLine("Read null input!", ConsoleColor.DarkRed);
                    Console.ReadLine();
                    return;
                }
                if(float.TryParse(strInterval, out interval)) {
                    break;
                } else {
                    ConsoleHelper.WriteLine("Wrong input format!", ConsoleColor.Red);
                }
            }

            Console.Clear();
            ConsoleHelper.WriteLine("SINGLE RANDOM SOLVER", ConsoleColor.DarkGray);
            Board.PrintBoardToConsole();

            ConsoleHelper.WriteLine($"Will click once every {interval} seconds", ConsoleColor.Cyan);
            ConsoleHelper.Write("Press ENTER to start...", ConsoleColor.Green);
            Console.ReadLine();

            while(!Board.Finished) {
                (int row, int col) = (rand.Next(1, Board.RowCount + 1), rand.Next(1, Board.ColCount + 1));
                Board.Click(row, col);

                Console.Clear();
                ConsoleHelper.WriteLine("SINGLE RANDOM SOLVER", ConsoleColor.DarkGray);
                Board.PrintBoardToConsole();

                ConsoleHelper.WriteLine($"Clicked position ({row}, {col})", ConsoleColor.Cyan);
                Thread.Sleep((int)(interval * 1000));
            }

            switch(Board.State) {
                default:
                    ConsoleHelper.WriteLine("Game... uh... finished??? What??????", ConsoleColor.Yellow);
                    break;
                case BoardState.Win:
                    ConsoleHelper.WriteLine("You win!", ConsoleColor.Green);
                    break;
                case BoardState.Lose:
                    ConsoleHelper.WriteLine("You lose!", ConsoleColor.Red);
                    break;
            }
            ConsoleHelper.Write("Press ENTER to continue...", ConsoleColor.Green);
            Console.ReadLine();
        }

        public void SolveImmediate(int attemptCount) {
            Console.Clear();
            ConsoleHelper.WriteLine("MASS RANDOM SOLVER", ConsoleColor.DarkGray);
            ConsoleHelper.Write("Solving board ", ConsoleColor.DarkGray);
            ConsoleHelper.Write($"{Board.RowCount}x{Board.ColCount}:{Board.MineCount}", ConsoleColor.Cyan);
            ConsoleHelper.Write(" in ", ConsoleColor.DarkGray);
            ConsoleHelper.Write(attemptCount.ToString(), ConsoleColor.Cyan);
            ConsoleHelper.WriteLine(" attempts", ConsoleColor.DarkGray);

            ConsoleHelper.Write("Press ENTER to start...", ConsoleColor.Green);
            Console.ReadLine();

            int winCount = 0;
            Random rand = new Random();
            for(int i = 1; i <= attemptCount; i++) {
                ConsoleHelper.Write($"Running attempt {i}... ", ConsoleColor.Gray);

                Board.Reset();
                while(!Board.Finished) {
                    (int row, int col) = (rand.Next(1, Board.RowCount + 1), rand.Next(1, Board.ColCount + 1));
                    Board.Click(row, col);
                }
                if(Board.State == BoardState.Win) {
                    winCount++;
                    ConsoleHelper.WriteLine("Win!", ConsoleColor.Green);
                } else {
                    ConsoleHelper.WriteLine("Lost.", ConsoleColor.Red);
                }
            }
            ConsoleHelper.Write("Finished mass solve: [", ConsoleColor.DarkGray);
            ConsoleHelper.Write(winCount.ToString(), ConsoleColor.Green);
            ConsoleHelper.Write("/", ConsoleColor.DarkGray);
            ConsoleHelper.Write(attemptCount.ToString(), ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("]", ConsoleColor.DarkGray);
            ConsoleHelper.Write("Win rate: ", ConsoleColor.DarkGray);
            ConsoleHelper.WriteLine($"{100 * (float)winCount / attemptCount}%", ConsoleColor.Green);

            ConsoleHelper.Write("Press ENTER to continue...", ConsoleColor.Green);
            Console.ReadLine();
        }
    }
}
