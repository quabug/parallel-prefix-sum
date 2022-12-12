using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    [BurstCompile]
    internal struct AddJob<TNumber, TValue> : IJobParallelFor
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
}