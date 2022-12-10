using System;
using UnityEngine;

public interface IPrefixSum : IDisposable
{
    ComputeBuffer Numbers { get; }
    ComputeBuffer Sums { get; }
    public void Dispatch();
}