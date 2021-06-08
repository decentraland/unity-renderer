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

        next.Select();
    }

    private void OnPreviousTrigger(DCLAction_Trigger action)
    {
        if (EventSystem.current.currentSelectedGameObject != gameObject)
            return;

        if (previous == null || !previous.interactable)
            return;

        previous.Select();
    }

}