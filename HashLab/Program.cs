using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace HashLab
{
    class Program
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);
        private static Stopwatch stopWatch = new Stopwatch();
        private static int tableWidth = 110;
        private static List<IHashFunction> hashFunctions = new List<IHashFunction>
        {
            new Stribog.Stribog(256)
        };

        static void Main(string[] args)
        {
            List<string> testMessages = new List<string>();
            List<string> rows = new List<string>
            {
                "Алгоритм"
            };

            Console.WriteLine("Генерируем строки...");
            for(int size = 10; size < 100000000; size *= 100)
            {
                testMessages.Add(GenerateRandomString(size));
                rows.Add(size.ToString());
            }

            Console.WriteLine("Начинается тестирование...");
            Console.WriteLine();
            PrintLine();
            PrintRow(rows.ToArray());
            PrintLine();

            List<string> functionResults = new List<string>();
            foreach (var function in hashFunctions)
            {
                functionResults.Add(function.Name);
                foreach (var message in testMessages)
                {
                    stopWatch.Restart();
                    function.GetHash(message);
                    stopWatch.Stop();

                    TimeSpan ts = stopWatch.Elapsed;

                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);

                    functionResults.Add(elapsedTime);
                }

                PrintRow(functionResults.ToArray());
            }

            PrintLine();
            Console.WriteLine();
            Console.WriteLine("Тестирование завершено.");

            Console.ReadKey();
        }

        private static string GenerateRandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                builder.Append(
                    Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))));
            }

            return builder.ToString();
        }

        private static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }

        private static void PrintRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCenter(column, width) + "|";
            }

            Console.WriteLine(row);
        }

        static string AlignCenter(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            return string.IsNullOrEmpty(text) 
                ? new string(' ', width) 
                : text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
        }
    }
}