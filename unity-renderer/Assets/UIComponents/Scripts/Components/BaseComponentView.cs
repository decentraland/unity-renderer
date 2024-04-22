using DCL;
using DCL.Helpers;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIComponents.Scripts.Components
{
    public abstract class BaseComponentView<TModel> : BaseComponentView, IBaseComponentView<TModel>
        where TModel: IEquatable<TModel>, new()
    {
        [field: SerializeField]
        protected TModel model { get; private set; } = new ();

        public void SetModel(TModel newModel)
        {
            if (!Equals(model, newModel))
            {
                model = newModel;
                RefreshControl();
            }
        }
    }
}

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
}

public interface IComponentModelConfig<T> where T: BaseComponentModel
{
    /// <summary>
    /// Fill the model and updates the component with this data.
    /// </summary>
    /// <param name="newModel">Data to configure the component.</param>
    void Configure(T newModel);
}

public abstract class BaseComponentView : MonoBehaviour, IBaseComponentView
{
    internal BaseComponentModel baseModel;
    private bool isDestroyed;

    public ShowHideAnimator showHideAnimator;
    public virtual bool isVisible { get; private set; }
    public bool isFocused { get; private set; }
    public bool isViewEnabled { get; set; } = true;
    public event Action<bool> onFocused;

    public virtual void Dispose()
    {
        DataStore.i.screen.size.OnChange -= OnScreenSizeModified;

        if (!isDestroyed && gameObject)
            Utils.SafeDestroy(gameObject);
    }

    public virtual void Awake()
    {
        showHideAnimator = GetComponent<ShowHideAnimator>();
        DataStore.i.screen.size.OnChange += OnScreenSizeModified;
    }

    public virtual void OnEnable()
    {
        if(!isViewEnabled) return;

        StartCoroutine(RaiseOnScreenSizeChangedAfterDelay());
    }

    public virtual void OnDisable()
    {
        if(!isViewEnabled) return;

        OnLoseFocus();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        Dispose();
    }

    public abstract void RefreshControl();

    public virtual void Show(bool instant = false)
    {
        if (!isViewEnabled || !showHideAnimator)
            return;

        showHideAnimator.Show(instant);
        isVisible = true;
    }

    public virtual void Hide(bool instant = false)
    {
        if (!isViewEnabled || !showHideAnimator)
            return;

        showHideAnimator.Hide(instant);
        isVisible = false;
    }

    public virtual void OnFocus()
    {
        if(!isViewEnabled) return;

        isFocused = true;
        onFocused?.Invoke(true);
    }

    public virtual void OnLoseFocus()
    {
        if(!isViewEnabled) return;

        isFocused = false;
        onFocused?.Invoke(false);
    }

    public virtual void OnScreenSizeChanged() { }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if(!isViewEnabled) return;

        OnFocus();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if(!isViewEnabled) return;

        OnLoseFocus();
    }

    private void OnScreenSizeModified(Vector2Int current, Vector2Int previous)
    {
        if (!isViewEnabled || !gameObject.activeInHierarchy)
            return;

        StartCoroutine(RaiseOnScreenSizeChangedAfterDelay());
    }

    private IEnumerator RaiseOnScreenSizeChangedAfterDelay()
    {
        yield return null;

        if(isViewEnabled)
            OnScreenSizeChanged();
    }

    public static T Create<T>(string resourceName) where T: BaseComponentView
    {
        T buttonComponentView = Instantiate(Resources.Load<GameObject>(resourceName)).GetComponent<T>();
        return buttonComponentView;
    }

#if UNITY_EDITOR
    public static T CreateUIComponentFromAssetDatabase<T>(string assetName) where T: BaseComponentView =>
        Instantiate(AssetDatabase.LoadAssetAtPath<T>($"Assets/UIComponents/Prefabs/{assetName}.prefab"));
#endif
}
