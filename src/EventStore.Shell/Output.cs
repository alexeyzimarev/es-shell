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

        public static void WriteValue(string key, object value)
        {
            if (value != null)
                Console.WriteLine($"{key}: {value}");
        }

        public static void WriteInfo(params string[] output)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Write(output);
            Console.ResetColor();
        }
        
        public static void WriteError(params string[] output)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(output);
            Console.ResetColor();
        }
        
        public static void WriteWarning(params string[] output)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write(output);
            Console.ResetColor();
        }
    }
}
