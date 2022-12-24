using UnityEngine;

public static class TestUtilities
{
    public static (int[] numbers, int[] sums) RandomNumbers(int seed)
    {
        var random = new System.Random(seed);
        var count = random.Next(1, 1000);
        Debug.Log($"count = {count}");
        var numbers = new int[count];
        for (var i = 0; i < count; i++) numbers[i] = random.Next(3);
        var sums = new int[numbers.Length];
        sums[0] = numbers[0];
        for (var i = 1; i < sums.Length; i++) sums[i] = sums[i - 1] + numbers[i];
        return (numbers, sums);
    }

    public static (float[] numbers, float[] sums) RandomFloatNumbers(int seed)
    {
        var random = new System.Random(seed);
        var count = random.Next(1, 1000);
        Debug.Log($"count = {count}");
        var numbers = new float[count];
        for (var i = 0; i < count; i++) numbers[i] = (float)random.NextDouble() * 3;
        var sums = new float[numbers.Length];
        sums[0] = numbers[0];
        for (var i = 1; i < sums.Length; i++) sums[i] = sums[i - 1] + numbers[i];
        return (numbers, sums);
    }
}