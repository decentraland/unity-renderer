using System;
using DCL;
using DCL.Builder;
using DCL.Controllers;
using UnityEngine;

public interface IBIWSaveController : IBIWController
{
    int GetSaveTimes();
    void SetSaveActivation(bool isActive, bool tryToSave = false);
    void TryToSave();
    void ForceSave();
}