using System;

namespace EventStore.Shell
{
    public static class Output
    {
        public static void Write(params string[] output)
        {
            foreach (var s in output)
            {
                Console.WriteLine(s);
            }
        }

        public static void WriteColoured(string[] output, ConsoleColor colour)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Write(output);
            Console.ForegroundColor = previous;
        }

        public static void WriteValue(string key, object value)
        {
            if (value != null)
                Console.WriteLine($"{key}: {value}");
        }

        public static void WriteInfo(params string[] output) => WriteColoured(output, ConsoleColor.Green);

        public static void WriteError(params string[] output) => WriteColoured(output, ConsoleColor.Red);

        public static void WriteWarning(params string[] output) => WriteColoured(output, ConsoleColor.Yellow);
        
        public static void WritePrompt(params string[] output) => WriteColoured(output, ConsoleColor.DarkBlue);
    }
}
