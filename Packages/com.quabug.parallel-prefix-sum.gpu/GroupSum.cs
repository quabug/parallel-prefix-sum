using UnityEngine;

namespace Parallel.GPU
{
    public class GroupSum
    {
        private readonly ComputeShader _shader;
        private readonly int _propertyGroupSums = Shader.PropertyToID("GroupSums");
        private readonly int _propertySums = Shader.PropertyToID("Sums");
        private readonly int _propertyCount = Shader.PropertyToID("Count");

        private readonly int _addGroupSumKernelIndex;
        private readonly int _collectGroupSumKernelIndex;
        private readonly uint _collectGroupSumKernelThreadGroupSize;

        public GroupSum(ComputeShader shader)
        {
            _shader = shader;
            _collectGroupSumKernelIndex = shader.FindKernel("CollectGroupSum");
            _addGroupSumKernelIndex = shader.FindKernel("AddGroupSum");
            shader.GetKernelThreadGroupSizes(_collectGroupSumKernelIndex, out _collectGroupSumKernelThreadGroupSize, out _, out _);
        }

        public void DispatchCollectGroupSum(ComputeBuffer sums, ComputeBuffer groupSums)
        {
            var groupCount = (groupSums.count + _collectGroupSumKernelThreadGroupSize - 1) / _collectGroupSumKernelThreadGroupSize;
            _shader.SetInt(_propertyCount, groupSums.count);
            _shader.SetBuffer(_collectGroupSumKernelIndex, _propertySums, sums);
            _shader.SetBuffer(_collectGroupSumKernelIndex, _propertyGroupSums, groupSums);
            _shader.Dispatch(_collectGroupSumKernelIndex, (int)groupCount, 1, 1);
        }

        public void DispatchAddGroupSum(ComputeBuffer sums, ComputeBuffer groupSums)
        {
            _shader.SetInt(_propertyCount, sums.count);
            _shader.SetBuffer(_addGroupSumKernelIndex, _propertySums, sums);
            _shader.SetBuffer(_addGroupSumKernelIndex, _propertyGroupSums, groupSums);
            _shader.Dispatch(_addGroupSumKernelIndex, groupSums.count, 1, 1);
        }
    }
}