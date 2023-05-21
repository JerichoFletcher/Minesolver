using Minesolver.CLI;
using Minesolver.Game;
using Minesolver.Utility;

namespace Minesolver.Solver {
    internal class ProbabilisticSolver : ISolver {
        private readonly object _lock = new object();

        public Board Board { get; }

        private readonly Random rand = new Random();

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
                Dictionary<(int, int), float> probabilityBoard = ComputeProbabilities(Board);

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
                Console.WriteLine($"Selecting {select} from {minProbabilitySquares.Length}");
                (int row, int col) = minProbabilitySquares[select];
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
            ConsoleHelper.WriteLine($"Queueing {attemptCount} worker threads...", ConsoleColor.Cyan);

            HashSet<Thread> threads = new HashSet<Thread>();
            for(int i = 1; i <= attemptCount; i++) {
                ThreadPool.QueueUserWorkItem(attemptNumber => {
                    Random rand = new Random();
                    int threadNumber = (int)(attemptNumber ?? rand.Next());
                    Board tBoard = new Board(Board.MineCount, Board.RowCount, Board.ColCount);

                    while(!tBoard.Finished) {
                        Dictionary<(int, int), float> probabilityBoard = ComputeProbabilities(tBoard);

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

                        try {
                            int select = rand.Next(minProbabilitySquares.Length);
                            (int row, int col) = minProbabilitySquares[select];
                            tBoard.Click(row, col);

                        } catch(Exception e) {
                            ReportResult(threadNumber, null, $"Array length: {minProbabilitySquares.Length}\nFull probability board:\n{string.Join('\n', probabilityBoard)}\n{e}");
                            return;
                        }
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
                coveredSquaresMined.Add((r, c), 0);
            }

            //Console.WriteLine("Board state:");
            //Console.WriteLine($"  {board.CoveredSquares.Count()} covered squares: {board.InnerCoveredSquares.Count()} inner + {board.OuterCoveredSquares.Count()} outer");
            //Console.WriteLine($"  {board.RevealedNumbers.Count()} revealed number squares");
            // Iterate through all combinations of mines
            int combCount = 0;
            int minOuterMineCount = Math.Max(0, board.RemainingMineCount - (board.CoveredSquares.Count() - board.OuterCoveredSquares.Count()));
            int maxOuterMineCount = Math.Min(board.OuterCoveredSquares.Count(), board.RemainingMineCount);
            for(int outerMineCount = minOuterMineCount; outerMineCount <= maxOuterMineCount; outerMineCount++) {
                int innerMineCount = board.RemainingMineCount - outerMineCount;
                int innerMineCombCount = board.InnerCoveredSquares.Count() != 0 ? Combinatorics.Combination(board.InnerCoveredSquares.Count(), innerMineCount) : 1;
                int innerMineCombOccurence = board.InnerCoveredSquares.Count() != 0 ? Combinatorics.Combination(board.InnerCoveredSquares.Count() - 1, innerMineCount - 1) : 0;
                //Console.WriteLine($"Checking for outer mine count = {outerMineCount}, {innerMineCount} inner mines combo {innerMineCombOccurence} occurence each in {innerMineCombCount} combinations");

                IEnumerable<(int mineRow, int mineCol)[]> outerMineConfigurations = board.OuterCoveredSquares.Combinations(outerMineCount);
                int validConfigs = 0;
                foreach(var outerMineComb in outerMineConfigurations.Select(array => new List<(int, int)>(array))) {
                    outerMineComb.AddRange(board.FlaggedSquares);
                    bool valid = true;

                    //Console.WriteLine($"Considering {string.Join(',', outerMineComb)}...");
                    // Verify count of mines
                    foreach((int numRow, int numCol) in board.RevealedNumbers) {
                        int mineCount = 0;
                        board.EachNeighbor(numRow, numCol, (checkRow, checkCol) => {
                            if(outerMineComb.Contains((checkRow, checkCol))) mineCount++;
                        });
                        if(mineCount != board.Peek(numRow, numCol)) {
                            valid = false;
                            break;
                        }
                    }
                    if(!valid) continue;
                    validConfigs++;

                    //Console.WriteLine($"Adding config {string.Join(',', outerMineComb)}");
                    // Add combination to the map
                    combCount += innerMineCombCount;
                    foreach((int, int) key in outerMineComb) {
                        coveredSquaresMined[key] += innerMineCombCount;
                    }
                }
                foreach((int, int) key in board.InnerCoveredSquares) {
                    coveredSquaresMined[key] += validConfigs * innerMineCombOccurence;
                }
            }

            // Calculate covered squares probability value
            foreach(KeyValuePair<(int r, int c), int> pair in coveredSquaresMined) {
                if(board.GetFlag(pair.Key.r, pair.Key.c)) {
                    probabilityBoard.Add(pair.Key, 1f);
                } else {
                    probabilityBoard.Add(pair.Key, combCount == 0 ? 0 : ((float)pair.Value / combCount));
                }
            }

            //Console.WriteLine($"Calculated:\n{string.Join('\n', probabilityBoard)}");
            //Console.ReadLine();
            return probabilityBoard;
        }
    }
}
