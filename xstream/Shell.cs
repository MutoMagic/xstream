using System;

namespace Xstream
{
    public static class Shell
    {
        public static void PressAnyKeyToContinue()
        {
            Console.Write("请按任意键继续. . .");
            Console.ReadKey();
        }

        public static void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public static void WriteLine(string output)
        {
            ConsoleColor dc = Console.ForegroundColor;

            Console.ForegroundColor = GetConsoleColor(output);
            Console.WriteLine(@"[{0}] {1}", DateTimeOffset.Now, output);

            Console.ForegroundColor = dc;
        }

        static ConsoleColor GetConsoleColor(string output)
        {
            if (output.StartsWith("Note")) return ConsoleColor.Green;
            if (output.StartsWith("Warning")) return ConsoleColor.Yellow;
            if (output.StartsWith("Error")) return ConsoleColor.Red;
            return Console.ForegroundColor;
        }
    }
}
