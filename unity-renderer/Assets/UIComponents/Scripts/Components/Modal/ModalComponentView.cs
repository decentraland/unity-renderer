using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IModalComponentView
{
    /// <summary>
    /// It will be triggered when the modal is closed.
    /// </summary>
    event Action OnCloseAction;

    /// <summary>
    /// Fill the model and updates the modal with this data.
    /// </summary>
    /// <param name="model">Data to configure the image.</param>
    void Configure(ModalComponentModel model);

    /// <summary>
    /// This will hide the close button of the pop up, forcing the user to take an action
    /// </summary>
    /// <param name="canBe"></param>
    void CanBeCancelled(bool canBe);
}

public class ModalComponentView : BaseComponentView, IModalComponentView
{
    public event Action OnCloseAction;

    [Header("Prefab References")]
    [SerializeField] internal ButtonComponentView closeButton;
    [SerializeField] internal Button alphaBackground;
    [SerializeField] internal GameObject container;

    [Header("Configuration")]
    [SerializeField] internal ModalComponentModel model;

    internal GameObject content;
    internal bool canBeCanceled = true;

    public void Start()
    {
        closeButton.onClick.AddListener(CloseButtonClicked);
        alphaBackground.onClick.AddListener(CloseButtonClicked);
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (content != null)
            Destroy(content);
        if (model.content != null)
            content = Instantiate(model.content, container.transform);
    }

    public void Configure(ModalComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public void CanBeCancelled(bool canBe)
    {
        closeButton.gameObject.SetActive(canBe);
        canBeCanceled = canBe;
    }

    internal void CloseButtonClicked()
    {
        if (!canBeCanceled)
            return;
        OnCloseAction?.Invoke();
        Hide();
    }

    public override void Show(bool instant = false)
    {
        if (isVisible)
            return;

        base.Show(instant);
    }
    public override void Hide(bool instant = false)
    {
        if (!isVisible)
            return;

        base.Hide(instant);
    }
}
