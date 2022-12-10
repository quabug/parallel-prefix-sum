using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Parallel.GPU
{
    public class ParallelPrefixSum : IPrefixSum
    {
        private readonly ComputeShader _shader;
        public ComputeBuffer Numbers { get; }
        public ComputeBuffer Sums { get; }

        private readonly int _prefixSumKernelIndex;
        private readonly List<ComputeBuffer> _inputNumbersBuffer = new();
        private readonly List<ComputeBuffer> _sumBuffers = new();

        private readonly int _addGroupSumKernelIndex;

        private readonly int _propertyCount = Shader.PropertyToID("Count");
        private readonly int _propertyNumbers = Shader.PropertyToID("Numbers");
        private readonly int _propertySums = Shader.PropertyToID("Sums");
        private readonly int _propertyGroupSums = Shader.PropertyToID("GroupSums");

        public ParallelPrefixSum(ComputeShader shader, int count)
        {
            _shader = shader;
            _shader = shader;

            Sums = new ComputeBuffer(count, UnsafeUtility.SizeOf<int>(), ComputeBufferType.Structured);
            Numbers = new ComputeBuffer(count, UnsafeUtility.SizeOf<int>(), ComputeBufferType.Structured);
            
            _inputNumbersBuffer.Add(Numbers);
            _sumBuffers.Add(Sums);

            _addGroupSumKernelIndex = shader.FindKernel("AddGroupSum");
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
                var numberBuffer = _inputNumbersBuffer[i];
                var sumBuffer = _sumBuffers[i];
                var groupSumBuffer = _inputNumbersBuffer[i + 1];
                _shader.SetInt(_propertyCount, numberBuffer.count);
                _shader.SetBuffer(_prefixSumKernelIndex, _propertyNumbers, numberBuffer);
                _shader.SetBuffer(_prefixSumKernelIndex, _propertySums, sumBuffer);
                _shader.SetBuffer(_prefixSumKernelIndex, _propertyGroupSums, groupSumBuffer);
                _shader.Dispatch(_prefixSumKernelIndex, groupSumBuffer.count, 1, 1);
            }

            for (var i = _inputNumbersBuffer.Count - 2; i >= 0; i--)
            {
                var sumBuffer = _sumBuffers[i];
                var groupSumBuffer = _sumBuffers[i + 1];
                _shader.SetInt(_propertyCount, sumBuffer.count);
                _shader.SetBuffer(_addGroupSumKernelIndex, _propertySums, sumBuffer);
                _shader.SetBuffer(_addGroupSumKernelIndex, _propertyGroupSums, groupSumBuffer);
                _shader.Dispatch(_addGroupSumKernelIndex, groupSumBuffer.count, 1, 1);
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