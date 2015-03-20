using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


namespace Suggestion
{
    public class Vocabulary
    {
        private Trie _words;
        private TcpClient _client;

        public bool CaseSensitive { get; set; }

        public Vocabulary(Dictionary<string, int> words, int limit = 0)
        {
            if (limit > 0)
                _words = new Trie(limit);
            else
                _words = new Trie();

            foreach(KeyValuePair<string, int> word in words)
            {
                _words.AddWord(CaseSensitive ? word.Key : word.Key.ToLower(), word.Value);
            }
        }
        
        public Vocabulary(string file)
            : this(LoadFromFile(File.OpenText(file)))
        {
        }

        /// <summary>
        /// Сетевой словарь
        /// </summary>
        /// <param name="addr"></param>
        public Vocabulary(IPEndPoint addr)
        {
            _client = new TcpClient();
            _client.Connect(addr);
            _client.ReceiveTimeout = 30;
        }

        /// <summary>
        /// запрос окончаний. Работает только с файлом
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public string[] GetComplitions(string prefix, int limit)
        {
            if (!CaseSensitive)
                prefix = prefix.ToLower();


            IList<string> completions = _words.GetCompletions(prefix, limit);

            return completions.ToArray();
        }

        /// <summary>
        /// запрос окончаний по сетив
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="limit"></param>в
        /// <param name="address"></param>
        /// <returns></returns>
        public string[] GetCompletionsByNetwork(string prefix, int limit, IPEndPoint address)
        {
            if (address == null)
                throw new ArgumentNullException("аддрес не задан");

            if (_client == null)
                _client = new TcpClient();

            if (!_client.Connected)
                _client.Connect(address);

            if (!_client.Connected)
            {
                _client.Connect(address);
            }

            byte[] message = Encoding.ASCII.GetBytes("get " + prefix + "\n\r");

            Stream stream = _client.GetStream();

            stream.Write(message, 0, message.Length);

            stream.Flush();

            byte[] answer = new byte[_client.ReceiveBufferSize];
            string answerStr = "";
            int count = stream.Read(answer, 0, _client.ReceiveBufferSize);

            while (count > 0)
            {
                answerStr += Encoding.ASCII.GetString(answer, 0, count);

                if (answerStr.IndexOf("\n\r\n\r") >= 0)
                    break;

                count = stream.Read(answer, 0, _client.ReceiveBufferSize);
            }

            List<string> completions = new List<string>();
            string[] words = Regex.Split(answerStr, "\n\r");
            foreach (string s in words)
                if (s.Length > 0)
                    completions.Add(s);

            return completions.ToArray();
        }

        static Dictionary<string, int> LoadFromFile(StreamReader fs)
        {
            Dictionary<string, int> words = new Dictionary<string, int>();

            //  читаем N
            string c = fs.ReadLine();
            int count = int.Parse(c);
            //  читаем слова
            while (count > 0 && !fs.EndOfStream)
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
