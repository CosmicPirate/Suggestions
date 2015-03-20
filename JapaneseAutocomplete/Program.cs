using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Suggestion;

namespace JapaneseAutocomplete
{
    class Program
    {
        static void Main(string[] args)
        {
//#if DEBUG
//            System.Diagnostics.Debugger.Launch();
//#endif

            TextReader reader = Console.In;

            if (args.Length >= 1)
            {
                if (args[0] == "b")
                {
                    Stopwatch watch = new Stopwatch();

                    watch.Start();
                    TestFile(reader, Console.Out, 10);
                    watch.Stop();

                    Console.Out.WriteLine();
                    Console.Out.WriteLine(watch.Elapsed + " " + watch.ElapsedMilliseconds + " " + watch.ElapsedTicks);
                }
                else if (args[0] == "t")
                {
                    double? avgMilliseconds = PerformanceTest(reader, Console.Out, 10, 10);
                    Console.Out.WriteLine(avgMilliseconds);
                }
            }
            else
            {
                TestFile(Console.In, Console.Out, 10);
            }
        }

        static void TestFile(TextReader input, TextWriter output, int limit)
        {
            //input.BaseStream.Position = 0; //  в начало файла
            output.Flush();

            Vocabulary vocab = new Vocabulary(LoadFromStream(input));

            //  чтение префиксов
            string countS = input.ReadLine();
            int count = int.Parse(countS);

            while (count-- > 0)
            {
                string prefix = input.ReadLine();

                string [] completions = vocab.GetComplitions(prefix, limit);
                foreach(string s in completions)
                {
                    output.WriteLine(s);
                }

                //if (completions.Count() > 0)
                    output.WriteLine();
            }
        }

        /// <summary>
        /// Средняя производительность ф-и TestFile
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="limit"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        static double? PerformanceTest(TextReader input, TextWriter output, int limit, int count)
        {
            Stopwatch watch = new Stopwatch();

            for (int i = 0; i < count; ++i)
            {
                watch.Start();
                TestFile(input, output, limit);
                watch.Stop();
            }

            return watch.ElapsedMilliseconds / count;
        }

        static Dictionary<string, int> LoadFromStream(TextReader fs)
        {
            Dictionary<string, int> words = new Dictionary<string, int>();

            //  читаем N
            string c = fs.ReadLine();
            int count = int.Parse(c);
            //  читаем слова
            while (count > 0)
            {
                LoadWordFromLine(fs.ReadLine(), words);
                --count;
            }

            return words;
        }

        static void LoadWordFromLine(string line, Dictionary<string, int> words)
        {
            string[] tokens = line.Split(' ');

            //  так как во входном файле гарантированно корректная информация,
            //  то можно ничего не проверять. А вообще желательно проверить
            words.Add(tokens[0], int.Parse(tokens[1]));
        }


    }

}
