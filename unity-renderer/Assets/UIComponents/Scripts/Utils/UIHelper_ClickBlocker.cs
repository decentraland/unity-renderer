using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a fullscreen blocker above all UIs.
/// It will ignore the specific UIs set in the 'nonBlockingCanvas' list.
/// </summary>
public class UIHelper_ClickBlocker : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] internal Canvas blockerCanvas;
    [SerializeField] internal RectTransform blockerTransform;
    [SerializeField] internal Button blockerButton;

    [Header("Optional Settings")]
    [SerializeField] internal List<Canvas> nonBlockingCanvas;

    /// <summary>
    /// Event that will be triggered when the blocker is clicked.
    /// </summary>
    public event Action OnClicked;

    internal Canvas parentcanvas = null;

    private void Awake()
    {
        blockerButton.onClick.AddListener(() => OnClicked?.Invoke());
    }

    private void OnDestroy()
    {
        blockerButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Activates the blocker.
    /// </summary>
    public void Activate()
    {
        if (parentcanvas == null)
            parentcanvas = GetComponentInParent<Canvas>();
        
        blockerTransform.SetParent(parentcanvas.rootCanvas.transform);

        blockerTransform.offsetMax = Vector2.zero;
        blockerTransform.offsetMin = Vector2.zero;
        gameObject.SetActive(true);
        foreach (Canvas canvas in nonBlockingCanvas)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = blockerCanvas.sortingOrder + 1;
        }
    }


    /// <summary>
    /// Deactivates the blocker.
    /// </summary>
    public void Deactivate()
    {
        gameObject.SetActive(false);
        foreach (Canvas canvas in nonBlockingCanvas)
        {
            canvas.overrideSorting = false;
        }
    }
}
