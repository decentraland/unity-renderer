using System;
using DCL.Builder;

public interface IBuilderMainPanelController
{
    /// <summary>
    /// If the user jump or edit a land this action will be released
    /// </summary>
    event Action OnJumpInOrEdit;

    /// <summary>
    /// Init the main panel of builder
    /// </summary>
    /// <param name="context"></param>
    void Initialize(IContext context);

    /// <summary>
    /// Dispose the main panel
    /// </summary>
    void Dispose();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isVisible"></param>
    void SetVisibility(bool isVisible);

}