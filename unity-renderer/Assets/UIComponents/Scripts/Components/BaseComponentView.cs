using DCL;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IBaseComponentView : IPointerEnterHandler, IPointerExitHandler, IDisposable
{
    /// <summary>
    /// It will be triggered after the UI component is fully initialized (PostInitialization included).
    /// </summary>
    event Action OnFullyInitialized;

    /// <summary>
    /// Returns true if the UI component is already fully initialized (PostInitialization included).
    /// </summary>
    bool isFullyInitialized { get; }

    /// <summary>
    /// It is called just after the UI component has been initialized.
    /// </summary>
    void PostInitialization();

    /// <summary>
    /// It is called every frame.
    /// </summary>
    void Update();

    /// <summary>
    /// It is called every frame (after Update).
    /// </summary>
    void LateUpdate();

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
    /// It is called when the focus is set into the component.
    /// </summary>
    void OnFocus();

    /// <summary>
    /// It is called when the focus is lost from the component.
    /// </summary>
    void OnLoseFocus();

    /// <summary>
    /// It is called just after the screen size has changed.
    /// </summary>
    void PostScreenSizeChanged();
}

[RequireComponent(typeof(ShowHideAnimator))]
[RequireComponent(typeof(CanvasGroup))]
public abstract class BaseComponentView : MonoBehaviour, IBaseComponentView
{
    public event Action OnFullyInitialized;
    public bool isFullyInitialized { get; private set; }

    internal ShowHideAnimator showHideAnimator;

    internal void Initialize()
    {
        isFullyInitialized = false;
        showHideAnimator = GetComponent<ShowHideAnimator>();

        DataStore.i.screen.size.OnChange += OnScreenSizeChange;
    }

    public abstract void PostInitialization();

    public virtual void Show(bool instant = false) { showHideAnimator.Show(instant); }

    public virtual void Hide(bool instant = false) { showHideAnimator.Hide(instant); }

    public abstract void RefreshControl();

    public virtual void OnFocus() { }

    public virtual void OnLoseFocus() { }

    public virtual void PostScreenSizeChanged() { }

    public virtual void Dispose() { DataStore.i.screen.size.OnChange -= OnScreenSizeChange; }

    private void OnEnable() { PostScreenSizeChanged(); }

    private void Awake() { Initialize(); }

    private void Start()
    {
        PostInitialization();

        isFullyInitialized = true;
        OnFullyInitialized?.Invoke();
    }

    public virtual void Update() { }

    public virtual void LateUpdate() { }

    public void OnPointerEnter(PointerEventData eventData) { OnFocus(); }

    public void OnPointerExit(PointerEventData eventData) { OnLoseFocus(); }

    private void OnDestroy() { Dispose(); }

    internal void OnScreenSizeChange(Vector2Int current, Vector2Int previous)
    {
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(RaisePostScreenSizeChangedAfterDelay());
    }

    internal IEnumerator RaisePostScreenSizeChangedAfterDelay()
    {
        yield return null;
        PostScreenSizeChanged();
    }

    internal static T Create<T>(string resourceName) where T : BaseComponentView
    {
        T buttonComponentView = Instantiate(Resources.Load<GameObject>(resourceName)).GetComponent<T>();
        return buttonComponentView;
    }
}