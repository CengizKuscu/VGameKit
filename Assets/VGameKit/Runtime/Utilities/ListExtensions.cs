using System.Collections.Generic;
using System.Linq;

namespace VGameKit.Runtime.Utilities
{
    public static class ListExtensions
    {
        /// <summary>
        /// Shuffles the elements of the list in place using UnityEngine.Random.
        /// </summary>
        /// <typeparam name="T">Type of list elements.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = UnityEngine.Random.Range(0, n);
                (list[k], list[n]) = (list[n], list[k]);
                // T value = list[k];
                // list[k] = list[n];
                // list[n] = value;
            }
        }

        /// <summary>
        /// Shuffles the elements of the list in place using a seeded System.Random instance.
        /// </summary>
        /// <typeparam name="T">Type of list elements.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        /// <param name="seed">Seed for the random number generator.</param>
        public static void Shuffle<T>(this IList<T> list, int seed)
        {
            var rng = new System.Random(seed);
            var n = list.Count;

            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
                // T value = list[k];
                // list[k] = list[n];
                // list[n] = value;
            }
        }

        /// <summary>
        /// Shuffles the elements of the list after the specified restriction index, leaving the first part unchanged.
        /// </summary>
        /// <typeparam name="T">Type of list elements.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        /// <param name="restrictionIndex">Index before which elements are not shuffled.</param>
        public static void ShuffleRestricted<T>(this IList<T> list, int restrictionIndex)
        {
            var firstPart = list.Take(restrictionIndex).ToList();
            var secondPart = list.Skip(restrictionIndex).ToList();

            secondPart.Shuffle();

            var tempList = firstPart.Concat(secondPart).ToList();

            for (var i = 0; i < tempList.Count; i++)
            {
                list[i] = tempList[i];
            }
        }

        /// <summary>
        /// Shuffles the elements of the list after the specified restriction index using a seeded random generator, leaving the first part unchanged.
        /// </summary>
        /// <typeparam name="T">Type of list elements.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        /// <param name="restrictionIndex">Index before which elements are not shuffled.</param>
        /// <param name="seed">Seed for the random number generator.</param>
        public static void ShuffleRestricted<T>(this IList<T> list, int restrictionIndex, int seed)
        {
            var firstPart = list.Take(restrictionIndex).ToList();
            var secondPart = list.Skip(restrictionIndex).ToList();

            secondPart.Shuffle(seed);

            var tempList = firstPart.Concat(secondPart).ToList();

            for (var i = 0; i < tempList.Count; i++)
            {
                list[i] = tempList[i];
            }
        }

        /// <summary>
        /// Returns a random item from the list using a seeded random generator.
        /// </summary>
        /// <typeparam name="T">Type of list elements.</typeparam>
        /// <param name="list">The list to select from.</param>
        /// <param name="seed">Seed for the random number generator.</param>
        /// <returns>A random item from the list.</returns>
        public static T GetRandomItem<T>(this IList<T> list, int seed)
        {
            var rng = new System.Random(seed);
            int n = list.Count;

            return list[rng.Next(n)];
        }

        /// <summary>
        /// Returns a new dictionary with the same keys but values in reverse order.
        /// </summary>
        /// <typeparam name="T">Type of dictionary keys.</typeparam>
        /// <typeparam name="K">Type of dictionary values.</typeparam>
        /// <param name="dict">The dictionary to reverse.</param>
        /// <returns>A new dictionary with values reversed.</returns>
        public static Dictionary<T, K> DictReverse<T, K>(this Dictionary<T, K> dict)
        {
            List<T> keys = new List<T>();
            List<K> values = new List<K>();

            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }

            values.Reverse();


            Dictionary<T, K> tempDict = new Dictionary<T, K>();

            for (int i = 0; i < keys.Count; i++)
            {
                tempDict.Add(keys[i], values[i]);
            }

            return tempDict;
        }
    }
}