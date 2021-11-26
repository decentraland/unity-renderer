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
}