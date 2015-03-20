using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Suggestion
{
    /// <summary>
    /// Префиксное дерево.
    /// </summary>
    public class Trie : ITrie
    {
        private TrieNode _root;
        private int _precomputedTopsCount = int.MaxValue;

        private ReaderWriterLock _rwLock = new ReaderWriterLock();

        public Trie()
        {
            _root = TrieNode.CreateRoot();
        }

        public Trie(int precomputedTopsCount) : this()
        {
            _precomputedTopsCount = precomputedTopsCount;
        }

        /// <summary>
        /// Добавить слово
        /// </summary>
        /// <param name="word"></param>
        public void AddWord(string word, int frequency)
        {
            try
            {
                _rwLock.AcquireWriterLock(10000);
                try
                {
                    TrieNode currentNode = _root;

                    foreach (char ch in word)
                    {
                        currentNode = currentNode.AddOrGetChild(ch);
                    }

                    currentNode.IsEndOfWord = true;
                    currentNode.Word = word;
                    currentNode.Weight = frequency;

                    UpdateTopSuffixes(currentNode, word);
                }
                finally
                {
                    _rwLock.ReleaseWriterLock();
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Находит все завершения для данного префикса, ограничивая по лимиту
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IList<string> GetCompletions(string prefix, int limit)
        {
            string[] result;

            try
            {
                _rwLock.AcquireReaderLock(10000);
                try
                {
                    TrieNode prefixLastNode = FindPrefix(prefix);

                    //  префикс не найден
                    if (prefixLastNode == null)
                        return new string[] { };

                    SortedSet<TrieNode> completions = prefixLastNode.TopmostCompletionsSet;

                    int resultsCount = Math.Min(limit, completions.Count());
                    result = new string[resultsCount];

                    int i = 0;
                    foreach (TrieNode node in completions)
                    {
                        if (i >= resultsCount)
                            break;
                        //result[i] = node.Word + " " + node.Weight;
                        result[i] = node.Word;
                        ++i;
                    }
                }
                catch { result = new string[] { }; }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
            catch (ApplicationException)
            {
                result = new string[] { };
            }

            return result;
        }

        /// <summary>
        /// Находит все завершения для данного префикса
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IList<string> GetCompletions(string prefix)
        {
            return GetCompletions(prefix, int.MaxValue);
        }

        /// <summary>
        /// Находит ноду по префиксу
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns>если префикс - пустая строка или содежит только пробелы возвращается корень дерева</returns>
        private TrieNode FindPrefix(string prefix)
        {
            //  если префикс можно считать пуcтой строкой возвращаем корень
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrWhiteSpace(prefix))
                return _root;

            TrieNode current = _root;

            foreach (char ch in prefix)
            {
                current = current.GetChild(ch);
                if (current == null)
                    return null;
            }

            return current;
        }

        /// <summary>
        /// Обновление списка суффиксов в ветке
        /// </summary>
        /// <param name="node">терминальная нода ветки</param>
        /// <param name="word"></param>
        private void UpdateTopSuffixes(TrieNode node, string word)
        {
            TrieNode current = node;
            while (current != null)
            {
                if (current.TopmostCompletionsSet.Count < _precomputedTopsCount || current.TopmostCompletionsSet.Last().CompareTo(node) < 0)
                {
                    current.TopmostCompletionsSet.Add(node);
                    current = current.Parent;
                }
            }
        }

    }

    /// <summary>
    /// нода
    /// </summary>
    internal class TrieNode : IComparable
    {
        private char _character;
        private int _weight;
        private SortedSet<TrieNode> _topSuffixes;
        private Dictionary<char, TrieNode> _children;

        #region geters setters
        
        /// <summary>
        /// Ценность ноды
        /// </summary>
        public int Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }
        
        /// <summary>
        /// СИмвол
        /// </summary>
        public char Character
        {
            get { return _character; }
            private set { _character = value; }
        }

        /// <summary>
        /// Массив самых "тяжелых" окончаний в поддереве как массив
        /// </summary>
        public TrieNode[] TopmostCompletions
        {
            get { return _topSuffixes.ToArray(); }
            set { _topSuffixes = new SortedSet<TrieNode>(value); }
        }

        /// <summary>
        /// Массив самых "тяжелых" окончаний в поддереве как есть
        /// </summary>
        public SortedSet<TrieNode> TopmostCompletionsSet
        {
            get { return _topSuffixes; }
            set { _topSuffixes = value; }
        }
        
        /// <summary>
        /// Дети ноды
        /// </summary>
        internal Dictionary<char, TrieNode> Children
        {
            get { return _children; }
            private set { _children = value; }
        }

        /// <summary>
        /// Терминальная ли нода
        /// </summary>
        public bool IsEndOfWord { get; set; }

        /// <summary>
        /// Слово полностью. Имеет смысл только если нода терминальная
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// Родитель
        /// </summary>
        public TrieNode Parent { get; set; }

        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="character"></param>
        protected TrieNode(char character)
        {
            _character = character;
            _children = new Dictionary<char,TrieNode>();
            _topSuffixes = new SortedSet<TrieNode>();
        }

        /// <summary>
        /// Добавляет потомка к ноду
        /// </summary>
        /// <param name="ch"></param>
        public TrieNode AddOrGetChild(char ch)
        {

            if (!_children.ContainsKey(ch))
            {
                TrieNode child = new TrieNode(ch);
                child.Parent = this;
                _children.Add(ch, child);
            }

            return _children[ch];
        }

        /// <summary>
        /// Удаляет потомка
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public bool RemoveChild(char ch)
        {
            if (_children.ContainsKey(ch))
            {
                _children.Remove(ch);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Выдает потомка ноды, если он есть
        /// </summary>
        /// <param name="ch"></param>
        /// <returns>null если нет</returns>
        public TrieNode GetChild(char ch)
        {
            if (!_children.ContainsKey(ch))
                return null;

            return _children[ch];
        }

        public int CompareTo(object obj)
        {
            if (_weight == ((TrieNode)obj).Weight)
                return Word.CompareTo(((TrieNode)obj).Word);

            return ((TrieNode)obj).Weight - Weight;
        }

        /// <summary>
        /// Создает пустую ноду, используемую в качестве корня
        /// </summary>
        /// <returns></returns>
        public static TrieNode CreateRoot()
        {
            return new TrieNode(' ');
        }

    }

}
