using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveHUDController
{
    void Initialize(ISaveHUDView publishBtnView);
    void Dispose();
    void SceneStateSave();
    void StopAnimation();
    void SetSaveViewByEntityListOpen(bool isOpen);
}

public class SaveHUDController : ISaveHUDController
{
    internal ISaveHUDView view;

    public void Initialize(ISaveHUDView saveView) { view = saveView; }

    public void Dispose() { }
    public void SceneStateSave() { view.SceneStateSaved(); }
    public void StopAnimation() { view.StopAnimation(); }

    public void SetSaveViewByEntityListOpen(bool isOpen) { view.SetViewByEntityListOpen(isOpen); }
}