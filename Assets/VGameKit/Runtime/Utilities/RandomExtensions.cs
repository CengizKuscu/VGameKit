namespace VGameKit.Runtime.Utilities
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Generates a random integer between the specified minimum and maximum values using the given seed.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator.</param>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random integer between min (inclusive) and max (exclusive).</returns>
        public static int Random(this int seed, int min, int max)
        {
            var rng = new System.Random(seed);
            return rng.Next(min, max);
        }

        /// <summary>
        /// Generates a random float between the specified minimum and maximum values using the given seed.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator.</param>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random float between min (inclusive) and max (exclusive).</returns>
        public static float Random(this int seed, float min, float max)
        {
            var rng = new System.Random(seed);
            return (float)rng.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Generates a random float between 0.0 and 1.0 using the given seed.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator.</param>
        /// <returns>A random float between 0.0 and 1.0.</returns>
        public static float Random(this int seed)
        {
            var rng = new System.Random(seed);
            return (float)rng.NextDouble();
        }

        /// <summary>
        /// Generates a random integer between the specified minimum and maximum values using a triangular distribution and the given seed.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator.</param>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        /// <param name="weight">The weight value used in the triangular distribution.</param>
        /// <returns>A random integer between min (inclusive) and max (exclusive) using a triangular distribution.</returns>
        public static int TriangularRandom(this int seed, int min, int max, double weight)
        {
            // Create a new Random object with the given seed value.
            System.Random random = new System.Random(seed);

            // Generate a random number using triangular distribution.
            int randomNumber = SampleTriangular(random, min, max, weight);

            return randomNumber;
        }

        /// <summary>
        /// Generates a random integer using a triangular distribution.
        /// </summary>
        /// <param name="random">The Random object used to generate random numbers.</param>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        /// <param name="weight">The weight value used in the triangular distribution.</param>
        /// <returns>A random integer between min (inclusive) and max (exclusive) using a triangular distribution.</returns>
        private static int SampleTriangular(System.Random random, int min, int max, double weight)
        {
            double u = random.NextDouble();
            //double c = weight / (max - min);
            double c = (weight - min) / (max - min);
            double x;

            if (u < c)
            {
                x = min + System.Math.Sqrt(u * (max - min) * (weight - min));
            }
            else
            {
                x = max - System.Math.Sqrt((1 - u) * (max - min) * (max - weight));
            }

            return (int)x;
        }


        /// <summary>
        /// Generates a random value from the specified values array based on the given weights using the provided seed.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator.</param>
        /// <param name="values">An array of possible values to choose from.</param>
        /// <param name="weights">An array of weights corresponding to the values.</param>
        /// <returns>A random value from the values array based on the weights.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the lengths of values and weights arrays do not match.</exception>
        public static int CumulativeRandom(this int seed, double[] values, double[] weights)
        {
            if (values.Length != weights.Length)
            {
                throw new System.ArgumentException("Values and weights arrays must have the same length.");
            }

            // Calculate cumulative weights.
            double[] cumulativeWeights = new double[weights.Length];
            cumulativeWeights[0] = weights[0];
            for (int i = 1; i < weights.Length; i++)
            {
                cumulativeWeights[i] = cumulativeWeights[i - 1] + weights[i];
            }

            // Generate a random number between 0 and the total weight.
            System.Random random = new System.Random(seed);
            double randomNumber = random.NextDouble() * cumulativeWeights[cumulativeWeights.Length - 1];

            // Find the corresponding value based on the random number.
            int index = System.Array.BinarySearch(cumulativeWeights, randomNumber);
            if (index < 0)
            {
                index = ~index; // Get the index of the next larger element.
            }

            return (int)values[index];
        }


        // <summary>
        /// Generates a weighted random integer between the specified minimum and maximum values,
        /// influenced by the target value and weight influence, using the given seed.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <param name="targetValue">The target value around which the random number is weighted.</param>
        /// <param name="weight">The influence of the weight on the random number, between 0 and 1.</param>
        /// <returns>A weighted random integer between minValue (inclusive) and maxValue (exclusive).</returns>
        /// <exception cref="System.ArgumentException">Thrown when the target value is outside the specified range or the weight is not between 0 and 1.</exception>
        public static int Roll(this int seed, int minValue, int maxValue, int targetValue, double weight)
        {
            // Validate input parameters
            if (targetValue < minValue || targetValue >= maxValue)
            {
                throw new System.ArgumentException($"Target value must be between {minValue} and {maxValue}.");
            }

            if (weight < 0 || weight > 1)
            {
                throw new System.ArgumentException("Weight must be between 0 and 1.");
            }
            
            System.Random random = new System.Random(seed);

            /*// Calculate the range
            int range = maxValue - minValue + 1;*/

            // Generate a random value within the range
            int randomValue = random.Next(minValue, maxValue);

            // Calculate the weighted value based on the distance from the target
            int weightedValue = (int)System.Math.Round(randomValue * (1 - weight) + targetValue * weight);

            /*int weightedMax = (int)System.Math.Round(randomValue * (1 - weight) + (targetValue * weight));
            int weightedMin = (int)System.Math.Round(randomValue * (1 - weight) - (targetValue * weight));*/

            // Ensure the weighted value stays within the specified range
            return System.Math.Max(minValue, System.Math.Min(maxValue-1, weightedValue));
        }
    }
}