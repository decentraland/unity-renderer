using UnityEngine;

public interface IBaseComponentView
{
    /// <summary>
    /// It is called when the UI component game object awakes.
    /// </summary>
    void Initialize();

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
    /// It is called when the UI component game object is destroyed.
    /// </summary>
    void Dispose();
}

[RequireComponent(typeof(ShowHideAnimator))]
public abstract class BaseComponentView : MonoBehaviour, IBaseComponentView
{
    private ShowHideAnimator showHideAnimator;

    public virtual void Initialize() { showHideAnimator = GetComponent<ShowHideAnimator>(); }

    public virtual void Show(bool instant = false) { showHideAnimator.Show(instant); }

    public virtual void Hide(bool instant = false) { showHideAnimator.Hide(instant); }

    public abstract void RefreshControl();

    public abstract void Dispose();

    private void Awake() { Initialize(); }
    private void OnDestroy() { Dispose(); }
    private void OnValidate() { RefreshControl(); }
}