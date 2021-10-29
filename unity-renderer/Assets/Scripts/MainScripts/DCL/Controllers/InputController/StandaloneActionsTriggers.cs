using DCL;
using UnityEngine;

/// <summary>
/// This class gathers all the triggers to orphan actions that doesn't belong to any subsystem or they just change the DataStore
/// </summary>
public class StandaloneActionsTriggers : MonoBehaviour
{
    [SerializeField] private InputAction_Trigger toggleAvatarNames;

    void Start() { toggleAvatarNames.OnTriggered += OnToggleAvatarNames; }

    private void OnDestroy() { toggleAvatarNames.OnTriggered -= OnToggleAvatarNames; }

    private void OnToggleAvatarNames(DCLAction_Trigger action) { DataStore.i.HUDs.avatarNamesVisible.Set(!DataStore.i.HUDs.avatarNamesVisible.Get()); }
}