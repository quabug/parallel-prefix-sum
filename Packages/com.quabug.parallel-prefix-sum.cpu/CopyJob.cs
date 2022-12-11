using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Parallel.CPU
{
    [BurstCompile]
    internal struct CopyJob<TValue> : IJobParallelFor where TValue : unmanaged
    {
        [ReadOnly] public NativeArray<TValue> Source;
        public NativeArray<TValue> Destination;

        public void Execute(int index)
        {
            Destination[index] = Source[index];
        }
    }
}