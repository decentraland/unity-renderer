using System;
using UnityEngine;

/// <summary>
/// An analogical action that raises events when the value is changed. The most common use-case is implementing axis
/// </summary>
[CreateAssetMenu(fileName = "InputAction_Measurable", menuName = "InputActions/Measurable")]
public class InputAction_Measurable : ScriptableObject
{
    public delegate void ValueChanged(DCLAction_Measurable action, float value);
    public event ValueChanged OnValueChanged;

    [SerializeField] internal DCLAction_Measurable dclAction;
    public DCLAction_Measurable GetDCLAction() => dclAction;

    [SerializeField] private float currentValue = 0;
    public float GetValue() => currentValue;

    internal void RaiseOnValueChanged(float value)
    {
        if (Math.Abs(currentValue - value) > Mathf.Epsilon)
        {
            currentValue = value;
            OnValueChanged?.Invoke(dclAction, currentValue);
        }
    }

    #region Editor

#if UNITY_EDITOR

    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        if (UnityEditor.AssetDatabase.Contains(this)) //It could happen that the SO has been created in runtime
            Resources.UnloadAsset(this);
    }

    [UnityEditor.CustomEditor(typeof(InputAction_Measurable), true)]
    internal class InputAction_MeasurableEditor : UnityEditor.Editor
    {
        private float changeValue = 0;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            changeValue = GUILayout.HorizontalSlider(changeValue, 0, 1);
            if (Application.isPlaying && GUILayout.Button("Raise OnChange"))
            {
                ((InputAction_Measurable)target).RaiseOnValueChanged(changeValue);
            }
        }
    }
#endif

    #endregion

}