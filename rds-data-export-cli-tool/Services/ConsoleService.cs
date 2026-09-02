using System;
using System.Text;

namespace RdsDataExportCliTool.Services {
    internal class ConsoleService : IConsoleService {
        public void Write(string message) {
            Console.Write(message);
        }

        public void WriteLine(string message) {
            Console.WriteLine(message);
        }

        public void WriteLine() {
            Console.WriteLine();
        }

        public string ReadPassword(string prompt) {
            Console.Write(prompt);
            if(Console.IsInputRedirected)
                return Console.ReadLine() ?? string.Empty;

            var builder = new StringBuilder();
            ConsoleKeyInfo key;
            while((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter) {
                if(key.Key == ConsoleKey.Backspace) {
                    if(builder.Length > 0) {
                        builder.Length--;
                        Console.Write("\b \b");
                    }
                } else if(!char.IsControl(key.KeyChar)) {
                    builder.Append(key.KeyChar);
                    Console.Write('*');
                }
            }

            Console.WriteLine();
            return builder.ToString();
        }
    }
}
