public interface IBaseVariable<T>
{
    event Change<T> OnChange;
    void Set(T value);
    T Get();
}
