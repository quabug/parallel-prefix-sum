using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Parallel.CPU
{
    public class ParallelPrefixSum<TNumber, TValue> : IPrefixSum<TValue>
        where TNumber : unmanaged, INumber<TValue>
        where TValue : unmanaged
    {
        public int BatchCount { get; set; } = 32;

        [BurstCompile]
        public struct PrefixSumStepJob : IJobParallelFor
        {
            public int Offset;
            [ReadOnly] public NativeArray<TValue> Values;
            public NativeArray<TValue> PrefixSums;

            public void Execute(int index)
            {
                Debug.Assert(Values.Length > 0);
                Debug.Assert(Values.Length == PrefixSums.Length);
                if (index < Offset) PrefixSums[index] = Values[index];
                else PrefixSums[index] = default(TNumber).Add(Values[index], Values[index - Offset]);
            }
        }

        [BurstCompile]
        public struct CopyJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<TValue> Source;
            public NativeArray<TValue> Destination;

            public void Execute(int index)
            {
                Destination[index] = Source[index];
            }
        }

        public JobHandle CalculatePrefixSum(NativeArray<TValue> inputValues, NativeArray<TValue> outputPrefixSum, JobHandle dependsOn = default)
        {
            var input = outputPrefixSum;
            var output = inputValues;

            for (var offset = 1; offset < input.Length; offset <<= 1)
            {
                (input, output) = (output, input);
                var handle = new PrefixSumStepJob { Offset = offset, Values = input, PrefixSums = output }
                    .Schedule(input.Length, BatchCount, dependsOn);
                dependsOn = JobHandle.CombineDependencies(dependsOn, handle);
            }
            if (output != outputPrefixSum)
            {
                var copyHandle = new CopyJob { Source = output, Destination = outputPrefixSum }
                    .Schedule(output.Length, BatchCount, dependsOn);
                dependsOn = JobHandle.CombineDependencies(dependsOn, copyHandle);
            }
            return dependsOn;
        }
    }
}