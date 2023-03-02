public interface IBaseVariable<T>
{
    event Change<T> OnChange;
    void Set(T value);
    void Set(T newValue, bool notifyEvent);
    T Get();
}