using System.Collections.Generic;
using UnityEngine;

public static class FisherYatesShuffle
{
    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static List<T> ShuffledCopy<T>(IList<T> original)
    {
        List<T> shuffled = new List<T>(original);
        Shuffle(shuffled);
        return shuffled;
    }
}