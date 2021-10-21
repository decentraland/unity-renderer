using DCL;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IBaseComponentView : IPointerEnterHandler, IPointerExitHandler, IDisposable
{
    /// <summary>
    /// It is called at the beginning of the UI component lifecycle.
    /// </summary>
    void OnAwake();

    /// <summary>
    /// It is called just after the UI component has been initialized.
    /// </summary>
    void OnStart();

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

    public virtual void OnAwake()
    {
        showHideAnimator = GetComponent<ShowHideAnimator>();
        DataStore.i.screen.size.OnChange += OnScreenSizeModified;
    }

    public virtual void OnStart() { }

    public abstract void RefreshControl();

    public virtual void Show(bool instant = false)
    {
        if (showHideAnimator == null)
            return;

        showHideAnimator.Show(instant);
    }

    public virtual void Hide(bool instant = false)
    {
        if (showHideAnimator == null)
            return;

        showHideAnimator.Hide(instant);
    }

    public virtual void OnFocus() { }

    public virtual void OnLoseFocus() { }

    public virtual void OnScreenSizeChanged() { }

    public virtual void Dispose() { DataStore.i.screen.size.OnChange -= OnScreenSizeModified; }

    private void Awake() { OnAwake(); }

    private void OnEnable() { OnScreenSizeChanged(); }

    private void Start() { OnStart(); }

    public void OnPointerEnter(PointerEventData eventData) { OnFocus(); }

    public void OnPointerExit(PointerEventData eventData) { OnLoseFocus(); }

    private void OnDestroy() { Dispose(); }

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