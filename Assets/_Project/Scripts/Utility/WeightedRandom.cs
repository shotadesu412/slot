using System;
using System.Collections.Generic;

namespace SlotRogue.Utility
{
    public static class WeightedRandom
    {
        public static T Pick<T>(List<T> items, List<float> weights, Random rng)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Items list cannot be empty");
            if (weights == null || weights.Count != items.Count)
                throw new ArgumentException("Weights must match items count");

            float totalWeight = 0;
            foreach (var w in weights)
                totalWeight += w;

            float roll = (float)(rng.NextDouble() * totalWeight);
            float cumulative = 0;

            for (int i = 0; i < items.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return items[i];
            }

            return items[items.Count - 1];
        }

        public static T Pick<T>(List<(T item, float weight)> weightedItems, Random rng)
        {
            if (weightedItems == null || weightedItems.Count == 0)
                throw new ArgumentException("List cannot be empty");

            float totalWeight = 0;
            foreach (var (_, weight) in weightedItems)
                totalWeight += weight;

            float roll = (float)(rng.NextDouble() * totalWeight);
            float cumulative = 0;

            foreach (var (item, weight) in weightedItems)
            {
                cumulative += weight;
                if (roll <= cumulative)
                    return item;
            }

            return weightedItems[weightedItems.Count - 1].item;
        }
    }
}
