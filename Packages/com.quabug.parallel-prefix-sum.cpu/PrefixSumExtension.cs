using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    public static class PrefixSumExtension
    {
        public static JobHandle CalculatePrefixSum<TNumber, TValue>(
            this NativeArray<TValue> inputValues,
            NativeArray<TValue> outputPrefixSum
        )
            where TNumber : unmanaged, INumber<TValue>
            where TValue : unmanaged
        {
            return new SingleThreadPrefixSum<TNumber, TValue>().CalculatePrefixSum(inputValues, outputPrefixSum);
        }

        public static JobHandle ParallelCalculatePrefixSum<TNumber, TValue>(
            this NativeArray<TValue> inputValues,
            NativeArray<TValue> outputPrefixSum,
            int batchCount = 32
        )
            where TNumber : unmanaged, INumber<TValue>
            where TValue : unmanaged
        {
            return new ParallelPrefixSum<TNumber, TValue>{ BatchCount = batchCount }
                .CalculatePrefixSum(inputValues, outputPrefixSum);
        }

        public static JobHandle WorkEfficientParallelCalculatePrefixSum<TNumber, TValue>(
            this NativeArray<TValue> inputValues,
            NativeArray<TValue> outputPrefixSum,
            int batchCount = 32
        )
            where TNumber : unmanaged, INumber<TValue>
            where TValue : unmanaged
        {
            return new WorkEfficientParallelPrefixSum<TNumber, TValue>{ BatchCount = batchCount }
                .CalculatePrefixSum(inputValues, outputPrefixSum);
        }
    }
}