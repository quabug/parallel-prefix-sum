namespace Parallel.CPU
{
    public interface INumber<T>
    {
        public T Add(T lhs, T rhs);
    }
}