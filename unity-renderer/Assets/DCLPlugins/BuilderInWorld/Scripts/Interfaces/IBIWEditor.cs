using DCL.Builder;
using DCL.Controllers;

public interface IBIWEditor
{
    void Initialize(IContext context);
    void Dispose();

    /// <summary>
    /// OnGUI unity call, don't use this one if it is not strictly necessary
    /// </summary>
    void OnGUI();

    /// <summary>
    /// Update unity callback
    /// </summary>
    void Update();

    /// <summary>
    /// LateUpdate unity callback
    /// </summary>
    void LateUpdate();

    /// <summary>
    /// Open the editor to start working on it
    /// </summary>
    void EnterEditMode(IBuilderScene scene);

    /// <summary>
    /// Exits from the editor
    /// </summary>
    void ExitEditMode();
}