using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    public interface IPrefixSum<TValue> where TValue : unmanaged
    {
        public JobHandle CalculatePrefixSum(
            NativeArray<TValue> inputValues,
            NativeArray<TValue> outputPrefixSum,
            JobHandle dependsOn = default
        );
    }
}