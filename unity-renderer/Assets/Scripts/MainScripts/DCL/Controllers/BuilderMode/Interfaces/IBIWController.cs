using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Controllers;
using UnityEngine;

public interface IBIWController
{
    void Initialize(IContext context);
    void EnterEditMode(IParcelScene scene);
    void ExitEditMode();
    void OnGUI();

    void LateUpdate();

    void Update();
    void Dispose();
}