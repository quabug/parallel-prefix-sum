using System.Linq;
using NUnit.Framework;
using Parallel.CPU;
using Parallel.GPU;
using Unity.Collections;
using Unity.PerformanceTesting;
using UnityEditor;
using UnityEngine;

[Category("benchmark")]
public class Benchmarks
{
    struct IntNumber : INumber<int>
    {
        public int Zero()
        {
            return 0;
        }

        public int Add(int lhs, int rhs)
        {
            return lhs + rhs;
        }
    }

    private static int[] _counts =
    {
        100, 1_000, 10_000, 100_000, 1_000_000
    };

    [Test, TestCaseSource(nameof(_counts)), Performance]
    public void CPU(int count)
    {
        {
            var values = new NativeArray<int>(count, Allocator.TempJob);
            var sums = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() =>
                {
                    sums[0] = values[0];
                    for (var i = 1; i < values.Length; i++) sums[i] = sums[i - 1] + values[i];
                })
                .SetUp(() =>
                {
                    for (var i = 0; i < values.Length; i++) values[i] = 1;
                })
                .SampleGroup("baseline")
                .Run()
            ;
            values.Dispose();
            sums.Dispose();
        }

        {
            var prefixSum = new SingleThreadPrefixSum<IntNumber, int>();
            var values = new NativeArray<int>(count, Allocator.TempJob);
            var sums = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() => prefixSum.CalculatePrefixSum(values, sums).Complete())
                .SampleGroup("single-thread")
                .SetUp(() =>
                {
                    for (var i = 0; i < values.Length; i++) values[i] = 1;
                })
                .Run()
            ;
            values.Dispose();
            sums.Dispose();
        }

        {
            var parallelPrefixSum = new ParallelPrefixSum<IntNumber, int>();
            var values = new NativeArray<int>(count, Allocator.TempJob);
            var sums = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() => parallelPrefixSum.CalculatePrefixSum(values, sums).Complete())
                .SampleGroup("parallel")
                .SetUp(() =>
                {
                    for (var i = 0; i < values.Length; i++) values[i] = 1;
                })
                .Run()
            ;
            values.Dispose();
            sums.Dispose();
        }

        {
            var parallelPrefixSum = new WorkEfficientParallelPrefixSum<IntNumber, int>();
            var values = new NativeArray<int>(count, Allocator.TempJob);
            var sums = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() => parallelPrefixSum.CalculatePrefixSum(values, sums).Complete())
                .SampleGroup("work-efficient-parallel")
                .SetUp(() =>
                {
                    for (var i = 0; i < values.Length; i++) values[i] = 1;
                })
                .Run()
            ;
            values.Dispose();
            sums.Dispose();
        }
    }

    [Test, TestCaseSource(nameof(_counts)), Performance]
    public void GPU(int count)
    {
        var sums = new int[count];
        var numbers = Enumerable.Repeat(1, count).ToArray();
        {
            var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/SingleThreadPrefixSum.compute");
            var prefixSum = new SingleThreadPrefixSum(shader, count);
            Measure.Method(() =>
                {
                    prefixSum.Dispatch();
                    prefixSum.Sums.GetData(sums);
                })
                .SampleGroup("single-threaded")
                .SetUp(() =>
                {
                    prefixSum.Numbers.SetData(numbers);
                })
                .Run()
            ;
        }

        {
            var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.quabug.parallel-prefix-sum.gpu/ParallelPrefixSum.compute");
            var parallelPrefixSum = new ParallelPrefixSum(shader, count);
            Measure.Method(() =>
                {
                    parallelPrefixSum.Dispatch();
                    parallelPrefixSum.Sums.GetData(sums);
                })
                .SampleGroup("parallel")
                .SetUp(() =>
                {
                    parallelPrefixSum.Numbers.SetData(numbers);
                })
                .Run()
            ;
        }
    }
}
