using UnityEngine;

/// <summary>
/// An instantaneous action which dispatches an event as soon as the input is read
/// </summary>
[CreateAssetMenu(fileName = "InputAction_Trigger", menuName = "InputActions/Trigger")]
public class InputAction_Trigger : ScriptableObject
{
    public delegate void Triggered(DCLAction_Trigger action);
    public event Triggered OnTriggered;

    [SerializeField] internal DCLAction_Trigger dclAction;
    public DCLAction_Trigger GetDCLAction() => dclAction;

    [SerializeField] internal BooleanVariable blockTrigger;
    public BooleanVariable isTriggerBlocked { get => blockTrigger; set => blockTrigger = value; }

    private int triggeredInFrame = -1;

    public bool WasTriggeredThisFrame()
    {
        return triggeredInFrame == Time.frameCount;
    }

    public void RaiseOnTriggered()
    {
        triggeredInFrame = Time.frameCount;
        OnTriggered?.Invoke(dclAction);
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

    [UnityEditor.CustomEditor(typeof(InputAction_Trigger), true)]
    internal class InputAction_TriggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying && GUILayout.Button("Raise OnChange"))
            {
                ((InputAction_Trigger)target).RaiseOnTriggered();
            }
        }
    }
#endif

    #endregion

}
