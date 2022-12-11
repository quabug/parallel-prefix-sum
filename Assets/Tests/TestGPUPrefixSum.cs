using System.Linq;
using NUnit.Framework;
using Parallel.GPU;
using UnityEditor;
using UnityEngine;

[Category("gpu")]
public class TestGPUPrefixSum
{
    [Test]
    public void should_calc_prefix_sum_in_single_thread([Random(0, 1000, 100)] int seed)
    {
        var prefix = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/SingleThreadPrefixSum.compute");
        var (numbers, sums) = TestUtilities.RandomNumbers(seed);
        using var prefixSum = new SingleThreadPrefixSum(prefix, numbers.Length);
        prefixSum.Numbers.SetData(numbers);
        prefixSum.Dispatch();
        var gpuSums = new int[sums.Length];
        prefixSum.Sums.GetData(gpuSums);
        Assert.That(sums, Is.EquivalentTo(gpuSums));
    }

    private static int[][] _cases =
    {
        new[] { 0, 0, 1, 5, 3, 0, 0, 1, 2, 7, 2, 0, 0, 0, 1, 1, 1 },
        Enumerable.Repeat(1, 128).ToArray(),
        Enumerable.Repeat(1, 10_000).ToArray(),
        Enumerable.Repeat(1, 100_000).ToArray(),
    };

    [Test, TestCaseSource(nameof(_cases))]
    public void should_calc_prefix_sum_in_parallel_for_simple_input(int[] numbers)
    {
        var prefix = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/ParallelPrefixSum.compute");
        var sums = new int[numbers.Length];
        sums[0] = numbers[0];
        for (var i = 1; i < numbers.Length; i++) sums[i] = sums[i - 1] + numbers[i];
        using var prefixSum = new ParallelPrefixSum(prefix, numbers.Length);
        prefixSum.Numbers.SetData(numbers);
        prefixSum.Dispatch();
        var gpuSums = new int[sums.Length];
        prefixSum.Sums.GetData(gpuSums);
        Assert.That(sums, Is.EquivalentTo(gpuSums));
    }

    [Test]
    public void should_calc_prefix_sum_in_parallel([Random(0, 1000, 100)] int seed)
    {
        var prefix = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/ParallelPrefixSum.compute");
        var (numbers, sums) = TestUtilities.RandomNumbers(seed);
        using var prefixSum = new ParallelPrefixSum(prefix, numbers.Length);
        prefixSum.Numbers.SetData(numbers);
        prefixSum.Dispatch();
        var gpuSums = new int[sums.Length];
        prefixSum.Sums.GetData(gpuSums);
        Assert.That(sums, Is.EquivalentTo(gpuSums));
    }
}