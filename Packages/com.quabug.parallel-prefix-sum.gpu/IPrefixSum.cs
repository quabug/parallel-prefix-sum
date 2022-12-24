using UnityEngine;

namespace Parallel.GPU
{
    public interface IPrefixSum
    {
        public ComputeBuffer Numbers { get; }
        public ComputeBuffer PrefixSums { get; }
        public void Dispatch();
    }
}
