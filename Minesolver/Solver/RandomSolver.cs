using Minesolver.CLI;
using Minesolver.Game;

namespace Minesolver.Solver {
    internal class RandomSolver {
        public Board Board { get; private set; }

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
            ConsoleHelper.WriteLine("RANDOM SOLVER", ConsoleColor.DarkGray);
            Board.PrintBoardToConsole();

            ConsoleHelper.WriteLine($"Will click once every {interval} seconds", ConsoleColor.Cyan);
            ConsoleHelper.Write("Press ENTER to start...", ConsoleColor.Cyan);
            Console.ReadLine();

            Random rand = new Random();
            while(!Board.Finished) {
                (int row, int col) pos = (rand.Next(0, Board.RowCount), rand.Next(0, Board.ColCount));
                int value = Board.Click(pos.row, pos.col);

                Console.Clear();
                ConsoleHelper.WriteLine("RANDOM SOLVER", ConsoleColor.DarkGray);
                Board.PrintBoardToConsole();

                ConsoleHelper.WriteLine($"Clicked position ({pos.row}, {pos.col})", ConsoleColor.Cyan);
                if(value == -1) break;
                if(!Board.Finished) Thread.Sleep((int)(interval * 1000));
            }

            ConsoleHelper.WriteLine("Game finished!", ConsoleColor.Green);
            Console.ReadLine();
        }
    }
}
