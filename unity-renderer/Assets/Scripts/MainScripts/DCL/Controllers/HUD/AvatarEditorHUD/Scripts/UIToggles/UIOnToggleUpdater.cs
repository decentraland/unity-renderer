using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Perform toggle action for all IToggleBehaviour components attached to the gameObject
/// when UI toggle state is changed
/// </summary>
[RequireComponent(typeof(Toggle))]
public class UIOnToggleUpdater : MonoBehaviour
{
    private Toggle toggle;

    private IUIToggleBehavior[] toggleBehaviours;

    protected void Awake()
    {
        toggleBehaviours = GetComponents<IUIToggleBehavior>();

        if (toggleBehaviours.Length == 0)
            Destroy(this);

        toggle = GetComponent<Toggle>();

        PerformToggleBehaviour(toggle.isOn);

        toggle.onValueChanged.AddListener(PerformToggleBehaviour);
    }

    private void OnDestroy() =>
        toggle.onValueChanged.RemoveListener(PerformToggleBehaviour);

    private void PerformToggleBehaviour(bool isOn)
    {
        foreach (IUIToggleBehavior toggleBehavior in toggleBehaviours)
            toggleBehavior.Toggle(isOn);
    }
}