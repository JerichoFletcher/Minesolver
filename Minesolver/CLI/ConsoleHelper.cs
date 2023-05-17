namespace Minesolver.CLI {
    internal static class ConsoleHelper {
        public static string? PromptLine(string prompt, ConsoleColor color) {
            Write(prompt, color);
            return Console.ReadLine();
        }

        public static void WriteLine(string message, ConsoleColor foreColor, ConsoleColor backColor) {
            ConsoleColor temp = Console.BackgroundColor;
            Console.BackgroundColor = backColor;
            WriteLine(message, foreColor);
            Console.BackgroundColor = temp;
        }

        public static void WriteLine(string message, ConsoleColor foreColor) {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = foreColor;
            Console.WriteLine(message);
            Console.ForegroundColor = temp;
        }

        public static void Write(string message, ConsoleColor foreColor, ConsoleColor backColor) {
            ConsoleColor temp = Console.BackgroundColor;
            Console.BackgroundColor = backColor;
            Write(message, foreColor);
            Console.BackgroundColor = temp;
        }

        public static void Write(string message, ConsoleColor foreColor) {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = foreColor;
            Console.Write(message);
            Console.ForegroundColor = temp;
        }
    }
}
