using DCL;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IBaseComponentView : IPointerEnterHandler, IPointerExitHandler, IDisposable
{
    /// <summary>
    /// It is called at the beggining of the UI component lyfe-cicle.
    /// </summary>
    void Initialization();

    /// <summary>
    /// It is called just after the UI component has been initialized.
    /// </summary>
    void PostInitialization();

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
    void PostScreenSizeChanged();
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

    public virtual void Initialization()
    {
        showHideAnimator = GetComponent<ShowHideAnimator>();
        DataStore.i.screen.size.OnChange += OnScreenSizeChange;
    }

    public virtual void PostInitialization() { }

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

    public virtual void PostScreenSizeChanged() { }

    public virtual void Dispose() { DataStore.i.screen.size.OnChange -= OnScreenSizeChange; }

    private void Awake() { Initialization(); }

    private void OnEnable() { PostScreenSizeChanged(); }

    private void Start() { PostInitialization(); }

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