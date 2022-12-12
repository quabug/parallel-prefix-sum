using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    public class ParallelPrefixSum<TNumber, TValue> : IPrefixSum<TValue>
        where TNumber : unmanaged, INumber<TValue>
        where TValue : unmanaged
    {
        public int BatchCount { get; set; } = 32;

        [BurstCompile]
        private struct ScanJob : IJobParallelFor
        {
            public int Offset;
            [ReadOnly] public NativeArray<TValue> Inputs;
            public NativeArray<TValue> Outputs;

            public void Execute(int index)
            {
                UnityEngine.Debug.Assert(Inputs.Length > 0);
                UnityEngine.Debug.Assert(Inputs.Length == Outputs.Length);
                if (index < Offset) Outputs[index] = Inputs[index];
                else Outputs[index] = default(TNumber).Add(Inputs[index], Inputs[index - Offset]);
            }
        }

        public JobHandle CalculatePrefixSum(NativeArray<TValue> inputValues, NativeArray<TValue> outputPrefixSum, JobHandle dependsOn = default)
        {
            var input = outputPrefixSum;
            var output = inputValues;

            for (var offset = 1; offset < input.Length; offset <<= 1)
            {
                (input, output) = (output, input);
                var handle = new ScanJob { Offset = offset, Inputs = input, Outputs = output }
                    .Schedule(input.Length, BatchCount, dependsOn);
                dependsOn = JobHandle.CombineDependencies(dependsOn, handle);
            }
            if (output != outputPrefixSum)
            {
                var copyHandle = new CopyJob<TValue> { Source = output, Destination = outputPrefixSum }
                    .Schedule(output.Length, BatchCount, dependsOn);
                dependsOn = JobHandle.CombineDependencies(dependsOn, copyHandle);
            }
            return dependsOn;
        }
    }
}