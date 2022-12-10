using System;
using UnityEngine;

namespace Parallel.GPU
{
    public interface IPrefixSum : IDisposable
    {
        ComputeBuffer Numbers { get; }
        ComputeBuffer Sums { get; }
        public void Dispatch();
    }
}
