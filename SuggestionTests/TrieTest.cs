using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Suggestion.UnitTests
{

    [TestFixture]
    public class TrieTest
    {
        [Test]
        public void OrderTest()
        {
            ITrie trie = new Trie();
            trie.AddWord("abc", 100);
            trie.AddWord("abb", 5);
            trie.AddWord("aaa", 99);
            trie.AddWord("acb", 8);
            trie.AddWord("bc", 200);
            trie.AddWord("ba", 200);

            IList<string> list = trie.GetCompletions("a");
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(list[0], "abc");
            Assert.AreEqual(list[1], "aaa");
            Assert.AreEqual(list[2], "acb");
            Assert.AreEqual(list[3], "abb");

            list = trie.GetCompletions("b");
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(list[0], "ba");
            Assert.AreEqual(list[1], "bc");
        }

        [Test]
        public void NotFoundTrieTest()
        {
            ITrie trie = new Trie();
            trie.AddWord("abc", 100);
            trie.AddWord("abb", 5);
            trie.AddWord("aca", 99);
            trie.AddWord("acb", 8);
            trie.AddWord("bc", 200);
            trie.AddWord("ba", 200);

            IList<string> list = trie.GetCompletions("xx");
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void SingleResultTrieTest()
        {
            var trie = new Trie();
            trie.AddWord("Donald Duck", 0);
            trie.AddWord("Duffy Duck", 0);
            trie.AddWord("Mini Mouse", 0);
            trie.AddWord("Mickey Mouse", 0);
            trie.AddWord("Pluto Dog", 0);
            trie.AddWord("Guffy Dog", 0);

            var list = trie.GetCompletions("Do");
            Assert.Contains("Donald Duck", (ICollection)list);
            Assert.AreEqual(1, list.Count);

            list = trie.GetCompletions("");
            Assert.AreEqual(6, list.Count);
        }

        [Test]
        public void OtherTests()
        {
            ITrie trie = new Trie();
            trie.AddWord("Abc", 100);
            trie.AddWord("abb", 5);
            trie.AddWord("aca", 99);
            trie.AddWord("acb", 8);
            trie.AddWord("bc", 200);
            trie.AddWord("ba", 200);
            trie.AddWord("a", 200);
            trie.AddWord("b", 200);
            trie.AddWord("c", 200);
            trie.AddWord("d", 200);
            trie.AddWord("e", 200);
            trie.AddWord("f", 200);
            trie.AddWord("g", 200);
            trie.AddWord("h", 200);
            trie.AddWord("i", 200);

            var list = trie.GetCompletions("A");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(list[0], "Abc");

            list = trie.GetCompletions("", 10);
            Assert.AreEqual(10, list.Count);
        }

    }
}
