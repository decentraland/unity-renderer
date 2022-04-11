using System;
using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DCL.Builder;
using DCL.Controllers;
using UnityEngine;

public interface IBIWActionController : IBIWController
{
    event System.Action OnRedo;
    event System.Action OnUndo;
    void AddAction(IBIWCompleteAction action);
    void TryToRedoAction();
    void TryToUndoAction();
    void CreateActionEntityDeleted(List<BIWEntity> entityList);
    void CreateActionEntityDeleted(BIWEntity entity);
    void CreateActionEntityCreated(IDCLEntity entity);
    void Clear();

    /// <summary>
    /// If any action has been done this will return true, however if any action has been done but all of them has been undo,
    /// this will return false
    /// </summary>
    /// <returns></returns>
    bool HasApplyAnyActionThisSession();

    /// <summary>
    /// This will return the last timestamp of the action
    /// </summary>
    /// <returns></returns>
    float GetLastActionTimestamp();
}