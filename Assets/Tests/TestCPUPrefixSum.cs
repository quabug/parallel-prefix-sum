using System.Linq;
using NUnit.Framework;
using Parallel.CPU;
using Unity.Collections;

public class TestCPUPrefixSum
{
    struct IntNumber : INumber<int>
    {
        public int Add(int lhs, int rhs)
        {
            return lhs + rhs;
        }
    }

    [Test]
    public void should_calc_prefix_sum_in_single_thread([Random(0, 1000, 100)] int seed)
    {
        var (numbers, sums) = TestUtilities.RandomNumbers(seed);
        using var nativeNumbers = new NativeArray<int>(numbers, Allocator.TempJob);
        using var nativeSums = new NativeArray<int>(numbers.Length, Allocator.TempJob);
        var prefixSum = new SingleThreadPrefixSum<IntNumber, int>();
        prefixSum.CalculatePrefixSum(nativeNumbers, nativeSums).Complete();
        Assert.That(sums, Is.EquivalentTo(nativeSums.ToArray()));
    }
    
    private static int[][] _cases =
    {
        new[] { 0, 0, 1, 5, 3, 0, 0, 1, 2, 7, 2, 0, 0, 0, 1, 1, 1 },
        new [] { 2,0,0,2,0,2,0,0,0,1,2,0,2,2,2,2,1,0,1,2,1,0,0,0,2,0,2,2,2 },
        Enumerable.Repeat(1, 29).ToArray(),
        Enumerable.Repeat(1, 128).ToArray(),
        Enumerable.Repeat(1, 10_000).ToArray(),
        Enumerable.Repeat(1, 100_000).ToArray(),
    };
    
    [Test, TestCaseSource(nameof(_cases))]
    public void should_calc_prefix_sum_in_parallel_for_simple_input(int[] numbers)
    {
        var sums = new int[numbers.Length];
        sums[0] = numbers[0];
        for (var i = 1; i < numbers.Length; i++) sums[i] = sums[i - 1] + numbers[i];
        var prefixSum = new ParallelPrefixSum<IntNumber, int>();
        using var nativeNumbers = new NativeArray<int>(numbers, Allocator.TempJob);
        using var nativeSums = new NativeArray<int>(numbers.Length, Allocator.TempJob);
        prefixSum.CalculatePrefixSum(nativeNumbers, nativeSums).Complete();
        Assert.That(sums, Is.EquivalentTo(nativeSums.ToArray()));
    }
    
    [Test]
    public void should_calc_prefix_sum_in_parallel([Random(0, 1000, 100)] int seed)
    {
        var (numbers, sums) = TestUtilities.RandomNumbers(seed);
        var prefixSum = new ParallelPrefixSum<IntNumber, int>();
        using var nativeNumbers = new NativeArray<int>(numbers, Allocator.TempJob);
        using var nativeSums = new NativeArray<int>(numbers.Length, Allocator.TempJob);
        prefixSum.CalculatePrefixSum(nativeNumbers, nativeSums).Complete();
        Assert.That(sums, Is.EquivalentTo(nativeSums.ToArray()));
    }
}