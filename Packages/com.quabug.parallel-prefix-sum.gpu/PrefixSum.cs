using System;
using UnityEngine;

namespace Parallel.GPU
{
    [CreateAssetMenu(fileName = "PrefixSum", menuName = "Parallel/GPU-PrefixSum", order = 0)]
    public sealed class PrefixSum : ScriptableObject, IPrefixSum
    {
        [SerializeField] private int _count = 512;
        [SerializeField] private int _stride = 4;
        [SerializeField] private ComputeShader _groupSumShader;
        [SerializeField] private ComputeShader _prefixSumShader;
        private Lazy<ParallelPrefixSum> _prefixSum;

        public ComputeBuffer Numbers => _prefixSum.Value.Numbers;
        public ComputeBuffer PrefixSums => _prefixSum.Value.PrefixSums;

        public PrefixSum()
        {
            SetCount(_count);
        }

        public void SetCount(int count)
        {
            OnDestroy();
            _prefixSum = new Lazy<ParallelPrefixSum>(() =>
                new ParallelPrefixSum(_prefixSumShader, new GroupSum(_groupSumShader), count, _stride));
            _count = count;
        }

        public void Dispatch()
        {
            _prefixSum.Value.Dispatch();
        }

        private void OnDestroy()
        {
            if (_prefixSum is { IsValueCreated: true }) _prefixSum.Value.Dispose();
        }
    }
}