﻿using Minesolver.Game;
using Minesolver.Solver;

namespace Minesolver.CLI {
    internal class Menu {
        public static bool MenuLoop() {
            Action<int, string> printOption = (int i, string name) => {
                ConsoleHelper.Write($"{i}. ", ConsoleColor.Green);
                ConsoleHelper.WriteLine(name, ConsoleColor.White);
            };

            Console.Clear();
            ConsoleHelper.WriteLine("MAIN MENU", ConsoleColor.DarkGray);
            printOption(1, "Play by yourself");
            printOption(2, "Single random solver");
            printOption(3, "Mass random solver");
            printOption(4, "Single probabilistic solver");
            printOption(5, "Mass probabilistic solver");
            printOption(0, "Exit");

            bool valid = false;
            while(!valid) {
                string? input = ConsoleHelper.PromptLine(">> ", ConsoleColor.DarkGray);
                if(input != null) {
                    if(input.Length > 0) {
                        if(int.TryParse(input, out int option)) {
                            switch(option) {
                                default:
                                    Board? board;
                                    ISolver solver;
                                    
                                    int attemptCount;

                                    ConsoleHelper.WriteLine($"Unknown option '{option}'! Try again.", ConsoleColor.Red);
                                    break;
                                case 1:
                                    // Manual solve
                                    valid = true;

                                    board = InputBoard();
                                    if(board == null) return false;

                                    while(PlayerGameLoop(board)) ;

                                    Console.Clear();
                                    ConsoleHelper.WriteLine("GAME", ConsoleColor.DarkGray);
                                    board.PrintBoardToConsole();

                                    switch(board.State) {
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
                                    Console.ReadLine();

                                    break;
                                case 2:
                                    // Single random solve
                                    valid = true;

                                    board = InputBoard();
                                    if(board == null) return false;

                                    solver = new RandomSolver(board);
                                    solver.Solve();

                                    break;
                                case 3:
                                    // Mass random solve
                                    valid = true;

                                    board = InputBoard();
                                    if(board == null) return false;

                                    while(true) {
                                        ConsoleHelper.Write("Attempt count: ", ConsoleColor.DarkGray);
                                        string? strAttemptCount = Console.ReadLine();
                                        if(strAttemptCount == null) {
                                            ConsoleHelper.WriteLine("Read null input!", ConsoleColor.DarkRed);
                                            return true;
                                        }

                                        if(int.TryParse(strAttemptCount, out attemptCount)) {
                                            break;
                                        } else {
                                            ConsoleHelper.WriteLine("Invalid input!", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    solver = new RandomSolver(board);
                                    solver.SolveImmediate(attemptCount);

                                    break;
                                case 4:
                                    // Single probabilistic solve
                                    valid = true;

                                    board = InputBoard();
                                    if(board == null) return false;

                                    solver = new ProbabilisticSolver(board);
                                    solver.Solve();

                                    break;
                                case 5:
                                    // Mass probabilistic solve
                                    valid = true;

                                    board = InputBoard();
                                    if(board == null) return false;

                                    while(true) {
                                        ConsoleHelper.Write("Attempt count: ", ConsoleColor.DarkGray);
                                        string? strAttemptCount = Console.ReadLine();
                                        if(strAttemptCount == null) {
                                            ConsoleHelper.WriteLine("Read null input!", ConsoleColor.DarkRed);
                                            return true;
                                        }

                                        if(int.TryParse(strAttemptCount, out attemptCount)) {
                                            break;
                                        } else {
                                            ConsoleHelper.WriteLine("Invalid input!", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    solver = new ProbabilisticSolver(board);
                                    solver.SolveImmediate(attemptCount);

                                    break;
                                case 0:
                                    // Stop the main loop
                                    ConsoleHelper.WriteLine("Thank you for using Minesolver!", ConsoleColor.White);
                                    return false;
                            }
                        } else {
                            ConsoleHelper.WriteLine($"Invalid input '{input}'! Try again.", ConsoleColor.Red);
                        }
                    }
                } else {
                    ConsoleHelper.WriteLine("Read null input!", ConsoleColor.DarkRed);
                    return false;
                }
            }

            Console.WriteLine();
            return true;
        }

        private static bool PlayerGameLoop(Board board) {
            Console.Clear();
            ConsoleHelper.WriteLine("GAME", ConsoleColor.DarkGray);
            board.PrintBoardToConsole();

            ConsoleHelper.Write("Pos: ", ConsoleColor.White);
            string? pos = Console.ReadLine();
            try {
                if(pos != null) {
                    string[] args = pos.Trim().Split(' ');
                    if(args.Length == 2) {
                        if(int.TryParse(args[0], out int row) && int.TryParse(args[1], out int col)) {
                            board.Click(row, col);
                        } else {
                            ConsoleHelper.WriteLine("Invalid input!", ConsoleColor.Red);
                            return true;
                        }
                    } else if(args.Length == 3) {
                        if(int.TryParse(args[0], out int row) && int.TryParse(args[1], out int col)) {
                            switch(args[2].ToLower()) {
                                case "f":
                                    board.SetFlag(row, col, !board.GetFlag(row, col));
                                    return true;
                                default:
                                    ConsoleHelper.WriteLine($"Unknown command '{args[2]}'!", ConsoleColor.Red);
                                    return true;
                            }
                        } else {
                            ConsoleHelper.WriteLine("Invalid input!", ConsoleColor.Red);
                            return true;
                        }
                    } else {
                        ConsoleHelper.WriteLine("Wrong input format!", ConsoleColor.Red);
                        return true;
                    }
                } else {
                    ConsoleHelper.WriteLine("Read null input!", ConsoleColor.DarkRed);
                    return false;
                }
            }catch(Exception e) {
                ConsoleHelper.WriteLine($"Error: {e.Message}", ConsoleColor.Red);
            }

            return !board.Finished;
        }

        private static Board? InputBoard() {
            // Read input
            bool boardValid = false;
            Board? board = null;
            while(!boardValid) {
                ConsoleHelper.Write("Board size: ", ConsoleColor.DarkGray);
                string? inputBoardSize = Console.ReadLine();
                if(inputBoardSize == null) {
                    ConsoleHelper.WriteLine("Read null input!", ConsoleColor.DarkRed);
                    return null;
                }

                string[] inputSplitSize = inputBoardSize.Split(' ');
                if(inputSplitSize.Length != 2) {
                    ConsoleHelper.WriteLine("Wrong input format!", ConsoleColor.Red);
                    continue;
                } else if(int.TryParse(inputSplitSize[0], out int rowCount) && int.TryParse(inputSplitSize[1], out int colCount)) {
                    ConsoleHelper.Write("Board mine count: ", ConsoleColor.DarkGray);
                    string? inputMineCount = Console.ReadLine();
                    if(inputMineCount == null) {
                        ConsoleHelper.WriteLine("Read null input!", ConsoleColor.DarkRed);
                        return null;
                    }

                    if(int.TryParse(inputMineCount, out int mineCount)) {
                        try {
                            board = new Board(mineCount, rowCount, colCount);
                            boardValid = true;
                        } catch(ArgumentOutOfRangeException e) {
                            boardValid = false;
                            ConsoleHelper.WriteLine($"Invalid board configuration: {e.Message}", ConsoleColor.Red);
                        }
                    } else {
                        ConsoleHelper.WriteLine("Invalid input!", ConsoleColor.Red);
                        continue;
                    }
                } else {
                    ConsoleHelper.WriteLine("Invalid input!", ConsoleColor.Red);
                    continue;
                }
            }
            if(board == null) throw new Exception("Null board");
            return board;
        }
    }
}
