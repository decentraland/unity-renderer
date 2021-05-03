/*
 * BaseTriggerVariable raise OnChange event everytime it value is set
 * regardless if the value is the same as it previous value.
 */
public class BaseTriggerVariable<T> : IBaseVariable<T>
{
    public event Change<T> OnChange;

    protected T value;

    public BaseTriggerVariable() { value = default; }
    public BaseTriggerVariable(T defaultValue) { value = defaultValue; }

    public T Get()
    {
        return value;
    }

    public void Set(T newValue)
    {
        var previous = value;
        value = newValue;
        OnChange?.Invoke(value, previous);
    }
}
