namespace Minesolver.CLI {
    internal static class ConsoleHelper {
        public static string? PromptLine(string prompt, ConsoleColor color) {
            Write(prompt, color);
            return Console.ReadLine();
        }

        public static void WriteLine(string message, ConsoleColor color) {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = temp;
        }

        public static void Write(string message, ConsoleColor color) {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = temp;
        }
    }
}
