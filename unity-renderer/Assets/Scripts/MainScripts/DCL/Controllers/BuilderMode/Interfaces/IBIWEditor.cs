using DCL.Builder;
using DCL.Controllers;

public interface IBIWEditor
{
    void Initialize(IContext context);
    void Dispose();
    void OnGUI();
    void Update();
    void LateUpdate();

    /// <summary>
    /// Open the editor with the scene that you pass. It must be ready to work correctly
    /// </summary>
    /// <param name="sceneToEdit"></param>
    void EnterEditMode(IParcelScene sceneToEdit);

    /// <summary>
    /// Exits from the editor
    /// </summary>
    void ExitEditMode();
}