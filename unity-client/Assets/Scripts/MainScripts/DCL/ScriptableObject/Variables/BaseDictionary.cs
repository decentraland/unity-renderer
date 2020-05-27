using System.Collections.Generic;
using UnityEngine;

public class BaseDictionary<TKey, TValue> : ScriptableObject
{
    public delegate void Added(TKey addedKey, TValue addedValue);

    public delegate void Removed(TKey removedKey, TValue removedValue);

    private event Added OnAddedElementValue;

    private event Removed OnRemovedElementValue;

    public virtual event Added OnAdded
    {
        add => OnAddedElementValue += value;
        remove => OnAddedElementValue -= value;
    }

    public virtual event Removed OnRemoved
    {
        add => OnRemovedElementValue += value;
        remove => OnRemovedElementValue -= value;
    }

    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public void Add(TKey newKey, TValue newValue)
    {
        if (dictionary.ContainsKey(newKey)) return;

        dictionary.Add(newKey, newValue);
        OnAddedElementValue?.Invoke(newKey, newValue);
    }

    public void Add(KeyValuePair<TKey, TValue>[] newValues)
    {
        int count = newValues.Length;
        for (int i = 0; i < count; ++i)
        {
            Add(newValues[i].Key, newValues[i].Value);
        }
    }

    public void Remove(TKey key)
    {
        if (!dictionary.ContainsKey(key)) return;

        var value = dictionary[key];
        dictionary.Remove(key);

        OnRemovedElementValue?.Invoke(key, value);
    }

    public void Remove(TKey[] keys)
    {
        int count = keys.Length;
        for (int i = 0; i < count; ++i)
        {
            Remove(keys[i]);
        }
    }

    public TValue Get(TKey key)
    {
        return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return dictionary.TryGetValue(key, out value);
    }

    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    public IEnumerable<TValue> GetValues()
    {
        return dictionary.Values;
    }

    public void Clear()
    {
        using (Dictionary<TKey, TValue>.Enumerator iterator = dictionary.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                OnRemovedElementValue?.Invoke(iterator.Current.Key, iterator.Current.Value);
            }
        }

        dictionary.Clear();
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        Resources.UnloadAsset(this);
    }
#endif
}