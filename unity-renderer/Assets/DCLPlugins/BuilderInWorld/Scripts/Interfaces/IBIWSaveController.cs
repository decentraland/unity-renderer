using System;
using DCL;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Controllers;
using UnityEngine;

public interface IBIWSaveController : IBIWController
{
    /// <summary>
    /// Get the amount of times that the scene has been saved
    /// </summary>
    /// <returns></returns>
    int GetSaveTimes();
    
    /// <summary>
    /// Enable or disables the save of the scene
    /// </summary>
    /// <param name="isActive"></param>
    /// <param name="tryToSave"></param>
    void SetSaveActivation(bool isActive, bool tryToSave = false);
    
    /// <summary>
    /// Try to save the scene is the conditions are met
    /// </summary>
    void TryToSave();
    
    /// <summary>
    /// This will save the scene no watter the conditions
    /// </summary>
    void ForceSave();

    /// <summary>
    /// This will set the manifest that will be used to save the scene
    /// </summary>
    /// <param name="manifest"></param>
    void SetManifest(Manifest manifest);
}