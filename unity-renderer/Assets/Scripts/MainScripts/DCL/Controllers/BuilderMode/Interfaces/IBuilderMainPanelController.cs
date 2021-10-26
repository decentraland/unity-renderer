using System;
using DCL.Builder;

public interface IBuilderMainPanelController
{
    event Action OnJumpInOrEdit;
    void Initialize(IContext context);
    void Dispose();
}