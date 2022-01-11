using DCL;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IBaseComponentView : IPointerEnterHandler, IPointerExitHandler, IDisposable
{
    /// <summary>
    /// It will inform if the UI Component is currently visible or not.
    /// </summary>
    bool isVisible { get; }

    /// <summary>
    /// It will be triggered when UI Component is focused.
    /// </summary>
    event Action<bool> onFocused;

    /// <summary>
    /// It will inform if the UI Component is focused or not.
    /// </summary>
    bool isFocused { get; }

    /// <summary>
    /// It is called at the beginning of the UI component lifecycle.
    /// </summary>
    void Awake();

    /// <summary>
    /// It is called each time the component is enabled.
    /// </summary>
    void OnEnable();

    /// <summary>
    /// It is called each time the component is disabled.
    /// </summary>
    void OnDisable();

    /// <summary>
    /// It is called just after the UI component has been initialized.
    /// </summary>
    void Start();

    /// <summary>
    /// It is called once per frame.
    /// </summary>
    void Update();

    /// <summary>
    /// Updates the UI component with the current model configuration.
    /// </summary>
    void RefreshControl();

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
    void OnScreenSizeChanged();
}

public interface IComponentModelConfig
{
    /// <summary>
    /// Fill the model and updates the component with this data.
    /// </summary>
    /// <param name="newModel">Data to configure the component.</param>
    void Configure(BaseComponentModel newModel);
}

public abstract class BaseComponentView : MonoBehaviour, IBaseComponentView
{
    internal BaseComponentModel baseModel;
    internal ShowHideAnimator showHideAnimator;

    public bool isVisible { get; private set; }
    private bool isDestroyed = false;

    public event Action<bool> onFocused;
    public bool isFocused { get; private set; }

    public virtual void Awake()
    {
        showHideAnimator = GetComponent<ShowHideAnimator>();
        DataStore.i.screen.size.OnChange += OnScreenSizeModified;
    }

    public virtual void OnEnable() { StartCoroutine(RaiseOnScreenSizeChangedAfterDelay()); }

    public virtual void OnDisable() { OnLoseFocus(); }

    public virtual void Start() { }

    public virtual void Update() { }

    public abstract void RefreshControl();

    public virtual void Show(bool instant = false)
    {
        if (showHideAnimator == null)
            return;

        showHideAnimator.Show(instant);
        isVisible = true;
    }

    public virtual void Hide(bool instant = false)
    {
        if (showHideAnimator == null)
            return;

        showHideAnimator.Hide(instant);
        isVisible = false;
    }

    public virtual void OnFocus()
    {
        isFocused = true;
        onFocused?.Invoke(true);
    }

    public virtual void OnLoseFocus()
    {
        isFocused = false;
        onFocused?.Invoke(false);
    }

    public virtual void OnScreenSizeChanged() { }

    public virtual void Dispose()
    {
        DataStore.i.screen.size.OnChange -= OnScreenSizeModified;
        if (!isDestroyed)
            Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData) { OnFocus(); }

    public void OnPointerExit(PointerEventData eventData) { OnLoseFocus(); }

    private void OnDestroy()
    {
        isDestroyed = true;
        Dispose();
    }

    internal void OnScreenSizeModified(Vector2Int current, Vector2Int previous)
    {
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(RaiseOnScreenSizeChangedAfterDelay());
    }

    internal IEnumerator RaiseOnScreenSizeChangedAfterDelay()
    {
        yield return null;
        OnScreenSizeChanged();
    }

    internal static T Create<T>(string resourceName) where T : BaseComponentView
    {
        T buttonComponentView = Instantiate(Resources.Load<GameObject>(resourceName)).GetComponent<T>();
        return buttonComponentView;
    }
}