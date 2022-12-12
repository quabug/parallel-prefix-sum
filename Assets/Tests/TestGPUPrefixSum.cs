using System.Linq;
using NUnit.Framework;
using Parallel.GPU;
using UnityEditor;
using UnityEngine;

[Category("gpu")]
public class TestGPUPrefixSum
{
    private GroupSum _groupSum;

    [SetUp]
    public void SetUp()
    {
        var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/GroupSum.compute");
        _groupSum = new GroupSum(shader);
    }

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
        Enumerable.Repeat(1, 7).ToArray(),
        Enumerable.Repeat(1, 8).ToArray(),
        Enumerable.Repeat(1, 9).ToArray(),
        Enumerable.Repeat(1, 32).ToArray(),
        Enumerable.Repeat(1, 128).ToArray(),
        new[] { 0, 0, 1, 5, 3, 0, 0, 1, 2, 7, 2, 0, 0, 0, 1, 1, 1 },
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
        using var prefixSum = new ParallelPrefixSum(prefix, numbers.Length, _groupSum);
        prefixSum.Numbers.SetData(numbers);
        prefixSum.Dispatch();
        var gpuSums = new int[sums.Length];
        prefixSum.Sums.GetData(gpuSums);
        Assert.That(sums, Is.EquivalentTo(gpuSums));
    }

    [Test]
    public void should_calc_prefix_sum_in_parallel_for_single_array()
    {
        var prefix = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/ParallelPrefixSum.compute");
        using var prefixSum = new ParallelPrefixSum(prefix, 1, _groupSum);
        prefixSum.Numbers.SetData(new [] { 123 });
        prefixSum.Dispatch();
        var gpuSums = new int[1];
        prefixSum.Sums.GetData(gpuSums);
        Assert.That(123, Is.EqualTo(gpuSums[0]));
    }

    [Test]
    public void should_calc_prefix_sum_in_parallel([Random(0, 1000, 100)] int seed)
    {
        var prefix = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/ParallelPrefixSum.compute");
        var (numbers, sums) = TestUtilities.RandomNumbers(seed);
        using var prefixSum = new ParallelPrefixSum(prefix, numbers.Length, _groupSum);
        prefixSum.Numbers.SetData(numbers);
        prefixSum.Dispatch();
        var gpuSums = new int[sums.Length];
        prefixSum.Sums.GetData(gpuSums);
        Assert.That(sums, Is.EquivalentTo(gpuSums));
    }

    [Test, TestCaseSource(nameof(_cases))]
    public void should_calc_prefix_sum_in_work_efficient_parallel_for_simple_input(int[] numbers)
    {
        var prefix = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/WorkEfficientParallelPrefixSum.compute");
        var sums = new int[numbers.Length];
        sums[0] = numbers[0];
        for (var i = 1; i < numbers.Length; i++) sums[i] = sums[i - 1] + numbers[i];
        using var prefixSum = new ParallelPrefixSum(prefix, numbers.Length, _groupSum);
        prefixSum.Numbers.SetData(numbers);
        prefixSum.Dispatch();
        var gpuSums = new int[sums.Length];
        prefixSum.Sums.GetData(gpuSums);
        Assert.That(sums, Is.EquivalentTo(gpuSums));
    }

    [Test]
    public void should_calc_prefix_sum_in_work_efficient_parallel([Random(0, 1000, 100)] int seed)
    {
        var prefix = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/WorkEfficientParallelPrefixSum.compute");
        var (numbers, sums) = TestUtilities.RandomNumbers(seed);
        using var prefixSum = new ParallelPrefixSum(prefix, numbers.Length, _groupSum);
        prefixSum.Numbers.SetData(numbers);
        prefixSum.Dispatch();
        var gpuSums = new int[sums.Length];
        prefixSum.Sums.GetData(gpuSums);
        Assert.That(sums, Is.EquivalentTo(gpuSums));
    }
}