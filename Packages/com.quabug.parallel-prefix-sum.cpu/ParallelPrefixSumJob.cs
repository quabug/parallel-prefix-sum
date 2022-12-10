using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    [BurstCompile]
    public struct ParallelPrefixSumJob<TNumber, TValue> : IJobParallelFor
        where TNumber : unmanaged, INumber<TValue>
        where TValue : unmanaged
    {
        public int Offset;
        [ReadOnly] public NativeArray<TValue> Values;
        public NativeArray<TValue> PrefixSums;

        public void Execute(int index)
        {
            UnityEngine.Debug.Assert(Values.Length > 0);
            UnityEngine.Debug.Assert(Values.Length == PrefixSums.Length);
            if (Offset < index) PrefixSums[index] = default(TNumber).Add(Values[index], Values[index - Offset]);
        }
    }
}