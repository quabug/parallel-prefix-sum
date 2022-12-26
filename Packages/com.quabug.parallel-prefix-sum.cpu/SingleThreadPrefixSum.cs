using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    public struct SingleThreadPrefixSum<TNumber, TValue> : IPrefixSum<TValue>
        where TNumber : unmanaged, INumber<TValue>
         where TValue : unmanaged
    {
        [BurstCompile]
        public struct Job : IJob 
        {
            [ReadOnly] public NativeArray<TValue> Values;
            public NativeArray<TValue> PrefixSum;

            public void Execute()
            {
                UnityEngine.Debug.Assert(Values.Length > 0);
                UnityEngine.Debug.Assert(Values.Length == PrefixSum.Length);
                PrefixSum[0] = Values[0];
                for (var i = 1; i < Values.Length; i++) PrefixSum[i] = default(TNumber).Add(PrefixSum[i - 1], Values[i]);
            }
        }

        public JobHandle CalculatePrefixSum(NativeArray<TValue> inputValues, NativeArray<TValue> outputPrefixSum, JobHandle dependsOn = default)
        {
            return new Job { Values = inputValues, PrefixSum = outputPrefixSum }.Schedule(dependsOn);
        }
    }
}