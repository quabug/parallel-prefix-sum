using System;
using UnityEngine;

namespace Parallel.GPU
{
    [Serializable]
    public sealed class SingleThreadPrefixSum : IPrefixSum, IDisposable
    {
        private readonly ComputeShader _shader;
        private readonly int _kernelIndex;

        public ComputeBuffer Numbers { get; }
        public ComputeBuffer PrefixSums { get; }

        public SingleThreadPrefixSum(ComputeShader shader, int count, int stride)
        {
            _shader = shader;
            _kernelIndex = shader.FindKernel("PrefixSum");

            PrefixSums = new ComputeBuffer(count, stride, ComputeBufferType.Structured);
            Numbers = new ComputeBuffer(count, stride, ComputeBufferType.Structured);

            _shader.SetInt("Count", count);
            _shader.SetBuffer(_kernelIndex, "Numbers", Numbers);
            _shader.SetBuffer(_kernelIndex, "Sums", PrefixSums);
        }

        public void Dispatch()
        {
            _shader.Dispatch(_kernelIndex, 1, 1, 1);
        }

        public void Dispose()
        {
            Numbers?.Dispose();
            PrefixSums?.Dispose();
        }
    }
}