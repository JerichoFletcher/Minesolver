using Minesolver.CLI;
using Minesolver.Game;
using Minesolver.Utility;

namespace Minesolver.Solver {
    internal class ProbabilisticSolver : ISolver {
        private readonly object _lock = new object();

        public Board Board { get; }

        private readonly Random rand = new Random();
        private Dictionary<(int, int), float> probabilityBoard = new Dictionary<(int, int), float>();

        private int attemptFinished, winCount;

        public ProbabilisticSolver(Board board) {
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
            ConsoleHelper.WriteLine("SINGLE PROBABILISTIC SOLVER", ConsoleColor.DarkGray);
            Board.PrintBoardToConsole();

            ConsoleHelper.WriteLine($"Will click once every {interval} seconds", ConsoleColor.Cyan);
            ConsoleHelper.Write("Press ENTER to start...", ConsoleColor.Green);
            Console.ReadLine();
            
            while(!Board.Finished) {
                probabilityBoard = ComputeProbabilities(Board);

                // Flag definitely mined squares
                IEnumerable<(int, int)> definiteMinedSquares = probabilityBoard
                    .Where(pair => pair.Value == 1f)
                    .Select(pair => pair.Key);
                foreach((int minedRow, int minedCol) in definiteMinedSquares) {
                    Board.SetFlag(minedRow, minedCol, true);
                }

                (int row, int col)[] minProbabilitySquares = probabilityBoard
                    .Where(pair => probabilityBoard.All(otherPair => pair.Value <= otherPair.Value))
                    .Select(pair => pair.Key)
                    .ToArray();

                int select = rand.Next(minProbabilitySquares.Length);
                (int row, int col) = minProbabilitySquares[select];
                //Console.WriteLine($"Result probability table:\n{string.Join('\n', probabilityBoard.Select(pair => $"{pair.Key}: {pair.Value}"))}");
                //Console.WriteLine($"Candidate squares:\n{string.Join('\n', minProbabilitySquares.Select(key => $"{key}: {probabilityBoard[key]}"))}");
                //Console.WriteLine($"Trying to click ({row}, {col}) [{probabilityBoard.First().Value} of index {select} in length {minProbabilitySquares.Length}]");
                Board.Click(row, col);

                Console.Clear();
                ConsoleHelper.WriteLine("SINGLE PROBABILISTIC SOLVER", ConsoleColor.DarkGray);
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
            ConsoleHelper.WriteLine("MASS PROBABILISTIC SOLVER", ConsoleColor.DarkGray);
            ConsoleHelper.Write("Solving board ", ConsoleColor.DarkGray);
            ConsoleHelper.Write($"{Board.RowCount}x{Board.ColCount}:{Board.MineCount}", ConsoleColor.Cyan);
            ConsoleHelper.Write(" in ", ConsoleColor.DarkGray);
            ConsoleHelper.Write(attemptCount.ToString(), ConsoleColor.Cyan);
            ConsoleHelper.WriteLine(" attempts", ConsoleColor.DarkGray);

            ConsoleHelper.Write("Press ENTER to start...", ConsoleColor.Green);
            Console.ReadLine();
            ConsoleHelper.WriteLine($"Starting {attemptCount} worker threads...", ConsoleColor.Cyan);

            HashSet<Thread> threads = new HashSet<Thread>();
            Random rand = new Random();
            for(int i = 1; i <= attemptCount; i++) {
                ThreadPool.QueueUserWorkItem(attemptNumber => {
                    int threadNumber = (int)(attemptNumber ?? rand.Next());
                    Board tBoard = new Board(Board.MineCount, Board.RowCount, Board.ColCount);

                    try {
                        while(!tBoard.Finished) {
                            probabilityBoard = ComputeProbabilities(tBoard);

                            // Flag definitely mined squares
                            IEnumerable<(int, int)> definiteMinedSquares = probabilityBoard
                                .Where(pair => pair.Value == 1f)
                                .Select(pair => pair.Key);
                            foreach((int minedRow, int minedCol) in definiteMinedSquares) {
                                tBoard.SetFlag(minedRow, minedCol, true);
                            }

                            (int row, int col)[] minProbabilitySquares = probabilityBoard
                                .Where(pair => probabilityBoard.All(otherPair => pair.Value <= otherPair.Value))
                                .Select(pair => pair.Key)
                                .ToArray();

                            int select = rand.Next(minProbabilitySquares.Length);
                            (int row, int col) = minProbabilitySquares[select];
                            //Console.WriteLine($"Result probability table:\n{string.Join('\n', probabilityBoard.Select(pair => $"{pair.Key}: {pair.Value}"))}");
                            //Console.WriteLine($"Candidate squares:\n{string.Join('\n', minProbabilitySquares.Select(key => $"{key}: {probabilityBoard[key]}"))}");
                            //Console.WriteLine($"Trying to click ({row}, {col}) [{probabilityBoard.First().Value} of index {select} in length {minProbabilitySquares.Length}]");
                            tBoard.Click(row, col);
                        }
                    } catch(Exception e) {
                        ReportResult(threadNumber, null, e.Message);
                    }

                    ReportResult(threadNumber, tBoard.State == BoardState.Win);
                }, i);
            }
            lock(this) {
                while(attemptFinished < attemptCount) Monitor.Wait(this);
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

        private void ReportResult(int threadNumber, bool? win, string message = "") {
            lock(this) {
                attemptFinished++;
                if(win == true) winCount++;
                ConsoleHelper.Write($"Thread {threadNumber} reported ", ConsoleColor.Gray);
                if(win == null) {
                    ConsoleHelper.Write("a failure.", ConsoleColor.DarkRed);
                } else if(win.Value) {
                    ConsoleHelper.Write("a win!", ConsoleColor.Green);
                } else {
                    ConsoleHelper.Write("a loss.", ConsoleColor.Red);
                }
                if(!string.IsNullOrEmpty(message)) ConsoleHelper.Write($" Note: {message}", ConsoleColor.DarkYellow);
                Console.WriteLine();

                Monitor.Pulse(this);
            }
        }

        private static Dictionary<(int, int), float> ComputeProbabilities(Board board) {
            Dictionary<(int, int), float> probabilityBoard = new Dictionary<(int, int), float>();
            Dictionary<(int, int), int> coveredSquaresMined = new Dictionary<(int, int), int>();

            // Search for all covered squares
            foreach((int r, int c) in board.CoveredSquares) {
                if(board.Peek(r, c) == null) {
                    coveredSquaresMined.Add((r, c), 0);
                }
            }

            // Iterate through all combinations of mines
            int combCount = 0;
            //Console.WriteLine($"Checking {Board.CoveredSquares.Combinations(Board.MineCount).Count()} of possible {Board.CoveredSquares.Combinations().Count()} configurations:");
            foreach((int mineRow, int mineCol)[] mineComb in board.CoveredSquares.Combinations(board.MineCount)) {
                bool valid = true;

                // Verify count of mines
                foreach((int numRow, int numCol) in board.RevealedNumbers) {
                    int mineCount = 0;
                    board.EachNeighbor(numRow, numCol, (checkRow, checkCol) => {
                        if(mineComb.Contains((checkRow, checkCol))) mineCount++;
                    });
                    if(mineCount != board.Peek(numRow, numCol)) {
                        valid = false;
                        break;
                    }
                }
                if(!valid) continue;

                // Add combination to the map
                //Console.WriteLine($"Adding configuration {string.Join(',', mineComb)}");
                combCount++;
                foreach((int, int) key in mineComb) {
                    //Console.Write($"    Adding count of {key} from {coveredSquaresMined[key]} to ");
                    coveredSquaresMined[key]++;
                    //Console.WriteLine(coveredSquaresMined[key]);
                }
                //Console.WriteLine($"");
            }

            // Calculate probability value
            foreach(KeyValuePair<(int, int), int> pair in coveredSquaresMined) {
                probabilityBoard.Add(pair.Key, (float)pair.Value / combCount);
            }

            return probabilityBoard;
        }
    }
}
