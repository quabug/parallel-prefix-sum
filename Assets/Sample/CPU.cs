using System.Linq;
using Parallel.CPU;
using Unity.Collections;
using UnityEngine;

public class CPU : MonoBehaviour
{
    [SerializeField] private int[] _numbers;
    [SerializeField] private int[] _prefixSums;
    
    private void Reset()
    {
        _numbers = Enumerable.Repeat(1, 1000).ToArray();
        _prefixSums = new int[1000];
    }
    
    private void Start()
    {
        using var numbers = new NativeArray<int>(_numbers, Allocator.TempJob);
        using var prefixSums = new NativeArray<int>(_prefixSums, Allocator.TempJob);
        new WorkEfficientParallelPrefixSum<IntNumber, int>().CalculatePrefixSum(numbers, prefixSums).Complete();
        _prefixSums = prefixSums.ToArray();
    }
    
    private struct IntNumber : INumber<int>
    {
        public int Zero()
        {
            return 0;
        }

        public int Add(int lhs, int rhs)
        {
            return lhs + rhs;
        }
    }
}