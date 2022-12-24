using NUnit.Framework;
using Parallel.CPU;
using Unity.Collections;
using Unity.Jobs;
using Unity.PerformanceTesting;

[Category("benchmark")]
public class JobsBenchmarks
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
        100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000//, 100_000_000, 1_000_000_000
    };

    [Test, TestCaseSource(nameof(_counts)), Performance]
    public void CopyJob(int count)
    {
        {
            var input = new NativeArray<int>(count, Allocator.TempJob);
            var output = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() =>
                {
                    for (var i = 0; i < input.Length; i++) output[i] = input[i];
                })
                .SetUp(() =>
                {
                    for (var i = 0; i < input.Length; i++) input[i] = 1;
                })
                .SampleGroup("baseline")
                .Run();
            input.Dispose();
            output.Dispose();
        }

        {
            var input = new NativeArray<int>(count, Allocator.TempJob);
            var output = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() =>
                {
                    new CopyJob<int> { Source = input, Destination = output }.Schedule().Complete();
                })
                .SetUp(() =>
                {
                    for (var i = 0; i < input.Length; i++) input[i] = 1;
                })
                .SampleGroup("copy")
                .Run();
            input.Dispose();
            output.Dispose();
        }

        {
            var input = new NativeArray<int>(count, Allocator.TempJob);
            var output = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() =>
                {
                    new ParallelCopyJob<int> { Source = input, Destination = output }.Schedule(count, 32).Complete();
                })
                .SetUp(() =>
                {
                    for (var i = 0; i < input.Length; i++) input[i] = 1;
                })
                .SampleGroup("copy-parallel")
                .Run();
            input.Dispose();
            output.Dispose();
        }
    }

    [Test, TestCaseSource(nameof(_counts)), Performance]
    public void AddJob(int count)
    {
        {
            var values = new NativeArray<int>(count, Allocator.TempJob);
            var output = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() =>
                {
                    for (var i = 0; i < values.Length; i++) output[i] += values[i];
                })
                .SetUp(() =>
                {
                    for (var i = 0; i < values.Length; i++) values[i] = 1;
                })
                .SampleGroup("baseline")
                .Run();
            values.Dispose();
            output.Dispose();
        }

        {
            var values = new NativeArray<int>(count, Allocator.TempJob);
            var output = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() =>
                {
                    new AddJob<IntNumber, int> { Values = values, AddValues = output }.Schedule().Complete();
                })
                .SetUp(() =>
                {
                    for (var i = 0; i < values.Length; i++) values[i] = 1;
                })
                .SampleGroup("add")
                .Run();
            values.Dispose();
            output.Dispose();
        }

        {
            var values = new NativeArray<int>(count, Allocator.TempJob);
            var output = new NativeArray<int>(count, Allocator.TempJob);
            Measure.Method(() =>
                {
                    new ParallelAddJob<IntNumber, int> { Values = values, AddValues = output }.Schedule(count, 32).Complete();
                })
                .SetUp(() =>
                {
                    for (var i = 0; i < values.Length; i++) values[i] = 1;
                })
                .SampleGroup("add-parallel")
                .Run();
            values.Dispose();
            output.Dispose();
        }
    }
}