using UnityEngine;

public static class ArrayExtension 
{
    /// <summary>
    /// Shuffles the contents of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array</typeparam>
    /// <param name="array">The array</param>
    public static void Shuffle<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = Random.Range(0, n - 1);
            n--;
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
