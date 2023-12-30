namespace Unichain.P2P;

/// <summary>
///    A simple object pool.
/// </summary>
/// <typeparam name="T">The type that this pool will hold</typeparam>
/// <param name="factory">A function to generate new objects</param>
/// <param name="maxPoolSize">The maximum amount of objects this pool can hold</param>
internal class Pool<T>(Func<T> factory, int maxPoolSize) {
    private readonly List<T> pool;

    public T Get() {
        if (pool.Count == 0) {
            return factory();
        }

        T item = pool[0];
        pool.RemoveAt(0);
        return item;
    }

    public void Return(T item) {
        if (pool.Count < maxPoolSize) {
            pool.Add(item);
        }
    }
}
