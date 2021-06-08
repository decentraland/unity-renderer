using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class NavigateWithAction : MonoBehaviour
{
    private Selectable selectable;

    [SerializeField] private Selectable previous;
    [SerializeField] private Selectable next;

    private InputAction_Trigger nextTrigger;
    private InputAction_Trigger previousTrigger;

    private Action actOnLateUpdate;

    private void Awake()
    {
        nextTrigger = Resources.Load<InputAction_Trigger>("UINavigationNext");
        previousTrigger = Resources.Load<InputAction_Trigger>("UINavigationPrevious");
    }

    private void OnEnable()
    {
        nextTrigger.OnTriggered += OnNextTrigger;
        previousTrigger.OnTriggered += OnPreviousTrigger;
    }

    private void OnDisable()
    {
        nextTrigger.OnTriggered -= OnNextTrigger;
        previousTrigger.OnTriggered -= OnPreviousTrigger;
    }

    private void OnNextTrigger(DCLAction_Trigger action)
    {
        if (EventSystem.current.currentSelectedGameObject != gameObject)
            return;

        if (next == null || !next.interactable)
            return;

        actOnLateUpdate = () => next.Select();
    }

    private void OnPreviousTrigger(DCLAction_Trigger action)
    {
        if (EventSystem.current.currentSelectedGameObject != gameObject)
            return;

        if (previous == null || !previous.interactable)
            return;

        actOnLateUpdate = () => previous.Select();
    }

    // We delay the navigation until the LateUpdate to prevent the following scenario:
    // UI_A <-> UI_B <-> UI_C 
    // Let's say UI_B subscribed to the next/previous triggers after UI_A.
    // 1) Next is triggered
    // 2) UI_A reacts and call next.Select() (now UI_B is currentSelectedGameObject)
    // 3) UI_B also reacts to the trigger and since now it's the currentSelected,
    //    it will perform a next.Select() as well
    private void LateUpdate()
    {
        if (actOnLateUpdate == null)
            return;

        actOnLateUpdate.Invoke();
        actOnLateUpdate = null;
    }

}