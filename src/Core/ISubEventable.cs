namespace Unichain.Core
{
    public interface ISubEventable<T>
    {
        public T SubEvent { get; set; }

    }
}
