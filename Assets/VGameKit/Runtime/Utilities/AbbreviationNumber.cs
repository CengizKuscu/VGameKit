using System.Collections.Generic;
using System.Linq;

namespace VGameKit.Runtime.Utilities
{
    /// <summary>
    /// Provides functionality to abbreviate large numbers using common suffixes (K, M, B).
    /// </summary>
    public class AbbreviationNumber
    {
        /// <summary>
        /// A sorted dictionary mapping numeric thresholds to their abbreviation suffixes.
        /// </summary>
        public static readonly SortedDictionary<int, string> Abbreviations = new SortedDictionary<int, string>
        {
            {10000, "K"}, {1000000, "M"}, {1000000000, "B"}
        };
        
        /// <summary>
        /// Abbreviates a given number using the defined suffixes.
        /// For example, 15000 becomes "1.5K".
        /// </summary>
        /// <param name="number">The number to abbreviate.</param>
        /// <returns>The abbreviated string representation of the number.</returns>
        public static string AbbreviateNumber(float number)
        {
            for (int i = Abbreviations.Count - 1; i >= 0; i--)
            {
                KeyValuePair<int, string> pair = Abbreviations.ElementAt(i);
                if (number >= pair.Key)
                {
                    float rest = number % pair.Key;
                    float k = (number - rest) / pair.Key;
                    float f = rest / (pair.Key / 10);
                    string roundedNumber;
                    if (f == 10)
                    {
                        f = 9;
                    }
                    roundedNumber = k.ToString() + "." + f.ToString();
                    return roundedNumber + pair.Value;
                }
            }
            return number.ToString();
        }
    }
}