namespace Parallel.CPU
{
    public interface INumber<T>
    {
        public T Zero();
        public T Add(T lhs, T rhs);
    }
}