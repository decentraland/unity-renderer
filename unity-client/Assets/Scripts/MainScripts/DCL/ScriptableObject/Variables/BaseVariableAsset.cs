using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseVariableAsset : ScriptableObject
{
#if UNITY_EDITOR
    protected abstract void RaiseOnChange();

    [UnityEditor.CustomEditor(typeof(BaseVariableAsset), true)]
    public class BaseVariableAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying && GUILayout.Button("Raise OnChange"))
            {
                ((BaseVariableAsset)target).RaiseOnChange();
            }
        }
    }
#endif
}

public class BaseVariableAsset<T> : BaseVariableAsset, IEquatable<T>
{
    public delegate void Change(T current, T previous);

    public event Change OnChange;

    [SerializeField] protected T value;

    public void Set(T newValue)
    {
        if (Equals(newValue))
            return;

        var previous = value;
        value = newValue;
        OnChange?.Invoke(value, previous);
    }

    public T Get()
    {
        return value;
    }

    public static implicit operator T(BaseVariableAsset<T> value) => value.value;

    public virtual bool Equals(T other)
    {
        //NOTE(Brian): According to benchmarks I made, this statement costs about twice than == operator for structs.
        //             However, its way more costly for primitives (tested only against float).

        //             Left here only for fallback purposes. Optimally this method should be always overriden.
        return EqualityComparer<T>.Default.Equals(value, other);
    }

#if UNITY_EDITOR
    protected sealed override void RaiseOnChange() => OnChange?.Invoke(value, value);

    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        if (UnityEditor.AssetDatabase.Contains(this)) //It could happen that the base variable has been created in runtime
            Resources.UnloadAsset(this);
    }
#endif
}