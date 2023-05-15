using Minesolver.Game;

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
            printOption(0, "Exit");

            bool valid = false;
            while(!valid) {
                string? input = ConsoleHelper.PromptLine(">> ", ConsoleColor.DarkGray);
                if(input != null) {
                    if(input.Length > 0) {
                        if(int.TryParse(input, out int option)) {
                            switch(option) {
                                default:
                                    ConsoleHelper.WriteLine($"Unknown option '{option}'! Try again.", ConsoleColor.Red);
                                    break;
                                case 1:
                                    valid = true;

                                    // Temp
                                    Board board = new Board(10, 10, 10);
                                    while(GameLoop(board)) ;

                                    ConsoleHelper.WriteLine("Game finished!", ConsoleColor.Green);
                                    Console.ReadLine();

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

        private static bool GameLoop(Board board) {
            Console.Clear();
            ConsoleHelper.WriteLine("GAME", ConsoleColor.DarkGray);

            board.PrintBoardToConsole();
            ConsoleHelper.Write("Pos: ", ConsoleColor.White);
            string? pos = Console.ReadLine();
            if(pos != null) {
                
            } else {
                ConsoleHelper.WriteLine("Read null input!", ConsoleColor.DarkRed);
                return false;
            }

            return !board.Finished;
        }
    }
}
