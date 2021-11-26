using DCL.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using UnityEngine;

public interface IBIWModeController : IBIWController
{
    public enum EditModeState
    {
        Inactive = 0,
        FirstPerson = 1,
        GodMode = 2
    }

    event Action<EditModeState, EditModeState> OnChangedEditModeState;
    event Action OnInputDone;
    Vector3 GetCurrentEditionPosition();
    void CreatedEntity(BIWEntity entity);

    float GetMaxDistanceToSelectEntities() ;

    void EntityDoubleClick(BIWEntity entity);

    Vector3 GetMousePosition();

    Vector3 GetModeCreationEntryPoint();
    bool IsGodModeActive();
    void UndoEditionGOLastStep();
    void StartMultiSelection();

    void EndMultiSelection();
    void CheckInput();

    void CheckInputSelectedEntities();

    bool ShouldCancelUndoAction();
    void MouseClickDetected();
}