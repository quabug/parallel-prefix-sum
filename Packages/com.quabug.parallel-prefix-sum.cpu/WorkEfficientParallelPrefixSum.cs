using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    public class WorkEfficientParallelPrefixSum<TNumber, TValue> : IPrefixSum<TValue>
        where TNumber : unmanaged, INumber<TValue>
        where TValue : unmanaged
    {
        public int BatchCount { get; set; } = 32;

        [BurstCompile]
        internal struct UpSweepScanJob : IJobParallelFor
        {
            public int AddOffset;
            [NativeDisableParallelForRestriction] public NativeArray<TValue> Values;

            public void Execute(int index)
            {
                var valueIndex = Values.Length - index * AddOffset * 2 - 1;
                var addIndex = valueIndex - AddOffset;
                Values[valueIndex] = default(TNumber).Add(Values[valueIndex], Values[addIndex]);
            }
        }

        [BurstCompile]
        internal struct SetLastZeroJob : IJob
        {
            public NativeArray<TValue> Values;
            public void Execute()
            {
                Values[^1] = default(TNumber).Zero();
            }
        }

        [BurstCompile]
        internal struct DownSweepScanJob : IJobParallelFor
        {
            public int AddOffset;
            [NativeDisableParallelForRestriction] public NativeArray<TValue> Values;

            public void Execute(int index)
            {
                var valueIndex = Values.Length - index * AddOffset * 2 - 1;
                var addIndex = valueIndex - AddOffset;
                var value = Values[valueIndex];
                Values[valueIndex] = default(TNumber).Add(value, Values[addIndex]);
                Values[addIndex] = value;
            }
        }

        public JobHandle CalculatePrefixSum(NativeArray<TValue> inputValues, NativeArray<TValue> outputPrefixSum, JobHandle dependsOn = default)
        {
            dependsOn = new ParallelCopyJob<TValue> { Source = inputValues, Destination = outputPrefixSum }.Schedule(inputValues.Length, BatchCount, dependsOn);
            var maxOffset = 0;
            (dependsOn, maxOffset) = UpSweep(outputPrefixSum, dependsOn);
            dependsOn = new SetLastZeroJob { Values = outputPrefixSum }.Schedule(dependsOn);
            dependsOn = DownSweep(outputPrefixSum, maxOffset, dependsOn);
            return new ParallelAddJob<TNumber, TValue> { AddValues = inputValues, Values = outputPrefixSum }.Schedule(inputValues.Length, BatchCount, dependsOn);
        }

        internal (JobHandle handle, int maxOffset) UpSweep(NativeArray<TValue> values, JobHandle dependsOn)
        {
            var offset = 1;
            for (; offset < values.Length; offset <<= 1)
            {
                var count = (values.Length + offset - 1) / (offset * 2);
                var handle = new UpSweepScanJob { AddOffset = offset, Values = values }.Schedule(count, BatchCount, dependsOn);
                dependsOn = JobHandle.CombineDependencies(handle, dependsOn);
            }
            return (dependsOn, offset);
        }

        internal JobHandle DownSweep(NativeArray<TValue> values, int offset, JobHandle dependsOn)
        {
            while (offset > 0)
            {
                var count = (values.Length + offset - 1) / (offset * 2);
                var handle = new DownSweepScanJob { AddOffset = offset, Values = values }.Schedule(count, BatchCount, dependsOn);
                dependsOn = JobHandle.CombineDependencies(handle, dependsOn);
                offset >>= 1;
            }
            return dependsOn;
        }
    }
}