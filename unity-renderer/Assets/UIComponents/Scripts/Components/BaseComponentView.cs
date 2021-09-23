using System;
using UnityEngine;

public interface IBaseComponentView
{
    /// <summary>
    /// It will be triggered after the UI component is initialized.
    /// </summary>
    event Action OnInitialized;

    /// <summary>
    /// Returns true if the UI component is already initialized.
    /// </summary>
    bool isInitialized { get; }

    /// <summary>
    /// It is called just after the UI component has been initialized.
    /// </summary>
    void PostInitialization();

    /// <summary>
    /// Shows the UI component.
    /// </summary>
    /// <param name="instant">True for not apply progressive animation.</param>
    void Show(bool instant = false);

    /// <summary>
    /// Hides the UI component.
    /// </summary>
    /// <param name="instant">True for not apply progressive animation.</param>
    void Hide(bool instant = false);

    /// <summary>
    /// Updates the UI component with the current model.
    /// </summary>
    void RefreshControl();

    /// <summary>
    /// It is called when the UI component is destroyed.
    /// </summary>
    void Dispose();
}

[RequireComponent(typeof(ShowHideAnimator))]
[RequireComponent(typeof(CanvasGroup))]
public abstract class BaseComponentView : MonoBehaviour, IBaseComponentView
{
    public event Action OnInitialized;
    public bool isInitialized { get; private set; }

    internal ShowHideAnimator showHideAnimator;

    internal void Initialize()
    {
        isInitialized = false;
        showHideAnimator = GetComponent<ShowHideAnimator>();

        PostInitialization();

        isInitialized = true;
        OnInitialized?.Invoke();
    }

    public abstract void PostInitialization();

    public virtual void Show(bool instant = false) { showHideAnimator.Show(instant); }

    public virtual void Hide(bool instant = false) { showHideAnimator.Hide(instant); }

    public abstract void RefreshControl();

    public virtual void Dispose() { }

    private void Awake() { Initialize(); }

    private void OnDestroy() { Dispose(); }

    internal static T Create<T>(string resourceName) where T : BaseComponentView
    {
        T buttonComponentView = Instantiate(Resources.Load<GameObject>(resourceName)).GetComponent<T>();
        return buttonComponentView;
    }
}