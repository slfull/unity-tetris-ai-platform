using System;
using System.Collections.Generic;
using System.Linq;

public static class IListExtension
{
    private static Random rng;

    public static void SetRandomSeed(int seed)
    {
        rng = new Random(seed);
    }
    public static void Shuffle<T>(this IList<T> list)
    {
        if (rng == null)
        {
            rng = new Random();
        }

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static bool IsNullOrEmpty<T>(this IList<T> enumerable)
    {
        if (enumerable == null) return true;
        return !enumerable.Any();
    }
}