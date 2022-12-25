using System.Linq;
using Parallel.GPU;
using UnityEngine;

public class GPU : MonoBehaviour
{
    [SerializeField] private PrefixSum _prefixSum;
    [SerializeField] private int[] _numbers;
    [SerializeField] private int[] _prefixSums;

    private void Reset()
    {
        _numbers = Enumerable.Repeat(1, 1000).ToArray();
        _prefixSums = new int[1000];
    }

    private void Start()
    {
        _prefixSum.SetCount(_numbers.Length);
        _prefixSum.Numbers.SetData(_numbers);
        _prefixSum.Dispatch();
        _prefixSum.PrefixSums.GetData(_prefixSums);
    }
}