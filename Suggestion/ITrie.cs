using System;
namespace Suggestion
{
    public interface ITrie
    {
        /// <summary>
        /// Добавляет слово
        /// </summary>
        /// <param name="word">собственно слово</param>
        /// <param name="frequency">частота употребления</param>
        void AddWord(string word, int frequency);

        /// <summary>
        /// Находит все завершения для данного префикса, ограничивая по лимиту
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        System.Collections.Generic.IList<string> GetCompletions(string prefix, int limit);

        /// <summary>
        /// Находит все завершения для данного префикса
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        System.Collections.Generic.IList<string> GetCompletions(string prefix);
    }
}
