using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Parallel.GPU
{
    [Serializable]
    public sealed class SingleThreadPrefixSum : IPrefixSum
    {
        private readonly ComputeShader _shader;
        private readonly int _kernelIndex;

        public ComputeBuffer Numbers { get; }
        public ComputeBuffer Sums { get; }

        public SingleThreadPrefixSum(ComputeShader shader, int count)
        {
            _shader = shader;
            _kernelIndex = shader.FindKernel("PrefixSum");

            Sums = new ComputeBuffer(count, UnsafeUtility.SizeOf<int>(), ComputeBufferType.Structured);
            Numbers = new ComputeBuffer(count, UnsafeUtility.SizeOf<int>(), ComputeBufferType.Structured);

            _shader.SetInt("Count", count);
            _shader.SetBuffer(_kernelIndex, "Numbers", Numbers);
            _shader.SetBuffer(_kernelIndex, "Sums", Sums);
        }

        public void Dispatch()
        {
            _shader.Dispatch(_kernelIndex, 1, 1, 1);
        }

        public void Dispose()
        {
            Numbers?.Dispose();
            Sums?.Dispose();
        }
    }
}