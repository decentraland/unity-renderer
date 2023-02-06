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
    public DCLAction_Trigger DCLAction => dclAction;

    [SerializeField] internal BooleanVariable blockTrigger;
    public BooleanVariable isTriggerBlocked { get => blockTrigger; set => blockTrigger = value; }

    private int triggeredInFrame = -1;

    public bool WasTriggeredThisFrame() =>
        triggeredInFrame == Time.frameCount;

    public void RaiseOnTriggered()
    {
        triggeredInFrame = Time.frameCount;
        OnTriggered?.Invoke(dclAction);
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(InputAction_Trigger), true)]
    internal class InputAction_TriggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying && GUILayout.Button("Raise OnChange"))
                ((InputAction_Trigger)target).RaiseOnTriggered();
        }
    }
#endif
}
