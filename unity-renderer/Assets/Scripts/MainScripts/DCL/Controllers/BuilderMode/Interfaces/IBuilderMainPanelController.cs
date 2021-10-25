using System;

public interface IBuilderMainPanelController
{
    event Action OnJumpInOrEdit;
    void Initialize();
    void Dispose();
}