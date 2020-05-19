using System;
using System.Collections.Generic;

namespace EventStore.Shell.ReadLine
{
    public static class ReadLine
    {
        static List<string> history;

        static ReadLine() => history = new List<string>();

        public static void AddHistory(params string[] text) => history.AddRange(text);
        public static List<string> GetHistory() => history;
        public static void ClearHistory() => history = new List<string>();

        public static bool HistoryEnabled { get; set; }

        public static IAutoCompleteHandler AutoCompletionHandler { private get; set; }

        public static string Read(string prompt = "", string @default = "")
        {
            Console.Write(prompt);
            var keyHandler = new KeyHandler(new Console2(), history, AutoCompletionHandler);

            var text = GetText(keyHandler);

            if (string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(@default))
            {
                text = @default;
            }
            else
            {
                if (HistoryEnabled) history.Add(text);
            }

            return text;
        }

        public static string ReadPassword(string prompt = "")
        {
            Console.Write(prompt);
            var keyHandler = new KeyHandler(new Console2 {PasswordMode = true}, null, null);
            return GetText(keyHandler);
        }

        static string GetText(KeyHandler keyHandler)
        {
            var keyInfo = Console.ReadKey(true);

            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyHandler.Handle(keyInfo);
                keyInfo = Console.ReadKey(true);
            }

            Console.WriteLine();
            return keyHandler.Text;
        }
    }
}
