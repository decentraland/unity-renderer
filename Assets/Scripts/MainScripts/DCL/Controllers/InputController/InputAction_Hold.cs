using System;
using UnityEngine;

/// <summary>
/// An on/off action that triggers Start when the input is read and Finished when the input has gone
/// </summary>
[CreateAssetMenu(fileName = "InputAction_Hold", menuName = "InputActions/Hold")]
public class InputAction_Hold : ScriptableObject
{
    public delegate void Started(DCLAction_Hold action);
    public delegate void Finished(DCLAction_Hold action);
    public event Started OnStarted;
    public event Finished OnFinished;

    [SerializeField] internal DCLAction_Hold dclAction;
    public DCLAction_Hold GetDCLAction() => dclAction;

    public bool isOn { get; private set; }

    public void RaiseOnStarted()
    {
        isOn = true;
        OnStarted?.Invoke(dclAction);
    }

    public void RaiseOnFinished()
    {
        isOn = false;
        OnFinished?.Invoke(dclAction);
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

    [UnityEditor.CustomEditor(typeof(InputAction_Hold), true)]
    internal class InputAction_HoldEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying && GUILayout.Button("Raise OnStarted"))
            {
                ((InputAction_Hold)target).RaiseOnStarted();
            }
            if (Application.isPlaying && GUILayout.Button("Raise OnFinished"))
            {
                ((InputAction_Hold)target).RaiseOnFinished();
            }
        }
    }
#endif

    #endregion

}