using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Parallel.GPU
{
    public class ParallelPrefixSum : IPrefixSum
    {
        private readonly ComputeShader _shader;
        private readonly GroupSum _groupSum;
        public ComputeBuffer Numbers { get; }
        public ComputeBuffer Sums { get; }

        private readonly int _prefixSumKernelIndex;
        private readonly List<ComputeBuffer> _inputNumbersBuffer = new();
        private readonly List<ComputeBuffer> _sumBuffers = new();

        private readonly int _propertyCount = Shader.PropertyToID("Count");
        private readonly int _propertyNumbers = Shader.PropertyToID("Numbers");
        private readonly int _propertySums = Shader.PropertyToID("Sums");

        public ParallelPrefixSum(ComputeShader shader, int count, GroupSum groupSum)
        {
            _shader = shader;
            _groupSum = groupSum;

            Sums = new ComputeBuffer(count, UnsafeUtility.SizeOf<int>(), ComputeBufferType.Structured);
            Numbers = new ComputeBuffer(count, UnsafeUtility.SizeOf<int>(), ComputeBufferType.Structured);
            
            _inputNumbersBuffer.Add(Numbers);
            _sumBuffers.Add(Sums);

            _prefixSumKernelIndex = shader.FindKernel("PrefixSum");
            var groupSize = count;
            shader.GetKernelThreadGroupSizes(_prefixSumKernelIndex, out var threadSize, out _, out _);
            while (groupSize > 1)
            {
                groupSize = (int) ((groupSize + (threadSize - 1)) / threadSize);
                _inputNumbersBuffer.Add(new ComputeBuffer(groupSize, UnsafeUtility.SizeOf<int>(), ComputeBufferType.Structured));
                _sumBuffers.Add(new ComputeBuffer(groupSize, UnsafeUtility.SizeOf<int>(), ComputeBufferType.Structured));
            }
        }

        public void Dispatch()
        {
            for (var i = 0; i < _inputNumbersBuffer.Count - 1; i++)
            {
                var groupSumBuffer = _inputNumbersBuffer[i + 1];
                var sumBuffer = DispatchStep(i, groupSumBuffer.count);
                _groupSum.DispatchCollectGroupSum(sums: sumBuffer, groupSums: groupSumBuffer);
            }
            // DispatchStep(_inputNumbersBuffer.Count - 1, 1);

            for (var i = _inputNumbersBuffer.Count - 2; i >= 0; i--)
            {
                var sumBuffer = _sumBuffers[i];
                var groupSumBuffer = _sumBuffers[i + 1];
                _groupSum.DispatchAddGroupSum(sums: sumBuffer, groupSums: groupSumBuffer);
            }

            ComputeBuffer DispatchStep(int step, int groupSize)
            {
                var numberBuffer = _inputNumbersBuffer[step];
                var sumBuffer = _sumBuffers[step];
                _shader.SetInt(_propertyCount, numberBuffer.count);
                _shader.SetBuffer(_prefixSumKernelIndex, _propertyNumbers, numberBuffer);
                _shader.SetBuffer(_prefixSumKernelIndex, _propertySums, sumBuffer);
                _shader.Dispatch(_prefixSumKernelIndex, groupSize, 1, 1);
                return sumBuffer;
            }
        }

        public void Dispose()
        {
            foreach (var buffer in _inputNumbersBuffer) buffer.Dispose();
            _inputNumbersBuffer.Clear();

            foreach (var buffer in _sumBuffers) buffer.Dispose();
            _sumBuffers.Clear();
        }
    }
}