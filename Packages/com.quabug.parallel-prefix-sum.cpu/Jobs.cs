using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    [BurstCompile]
    internal struct ParallelCopyJob<TValue> : IJobParallelFor where TValue : unmanaged
    {
        [ReadOnly] public NativeArray<TValue> Source;
        public NativeArray<TValue> Destination;

        public void Execute(int index)
        {
            Destination[index] = Source[index];
        }
    }

    [BurstCompile]
    internal struct CopyJob<TValue> : IJob where TValue : unmanaged
    {
        [ReadOnly] public NativeArray<TValue> Source;
        public NativeArray<TValue> Destination;

        public void Execute()
        {
            for (var i = 0; i < Source.Length; i++)
            {
                Destination[i] = Source[i];
            }
        }
    }

    [BurstCompile]
    internal struct ParallelAddJob<TNumber, TValue> : IJobParallelFor
        where TNumber : unmanaged, INumber<TValue>
        where TValue : unmanaged
    {
        [ReadOnly] public NativeArray<TValue> AddValues;
        public NativeArray<TValue> Values;

        public void Execute(int index)
        {
            Values[index] = default(TNumber).Add(Values[index], AddValues[index]);
        }
    }

    [BurstCompile]
    internal struct AddJob<TNumber, TValue> : IJob
        where TNumber : unmanaged, INumber<TValue>
        where TValue : unmanaged
    {
        [ReadOnly] public NativeArray<TValue> AddValues;
        public NativeArray<TValue> Values;

        public void Execute()
        {
            for (var i = 0; i < AddValues.Length; i++)
            {
                Values[i] = default(TNumber).Add(Values[i], AddValues[i]);
            }
        }
    }
}