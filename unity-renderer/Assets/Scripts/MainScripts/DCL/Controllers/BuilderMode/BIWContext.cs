using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class BIWContext
{
    private const string PROJECT_REFERENCES_PATH = "ScriptableObjects/ProjectReferences";
    private const string GOD_MODE_DYNAMIC_VARIABLE_PATH = "ScriptableObjects/GodModeVariables";
    private const string FIRST_PERSON_DYNAMIC_VARIABLE_PATH = "ScriptableObjects/FirstPersonVariables";
    private const string INPUTS_PATH = "ScriptableObjects/InputReferences";

    //Scriptable Objects
    public BIWProjectReferences projectReferencesAsset { get; private set; }
    public BIWGodModeDynamicVariables godModeDynamicVariablesAsset { get; private set; }
    public BIWFirstPersonDynamicVariables firstPersonDynamicVariablesAsset { get; private set; }
    public BIWInputsReferences inputsReferencesAsset { get; private set; }

    // BIW Controllers
    public IBIWOutlinerController outlinerController { get; private set; }
    public IBIWInputHandler inputHandler { get; private set; }
    public IBIWInputWrapper inputWrapper { get; private set; }
    public IBIWPublishController publishController { get; private set; }
    public IBIWCreatorController creatorController { get; private set; }
    public IBIWModeController modeController { get; private set; }
    public IBIWFloorHandler floorHandler { get; private set; }
    public IBIWEntityHandler entityHandler { get; private set; }
    public IBIWActionController actionController { get; private set; }
    public IBIWSaveController saveController { get; private set; }
    public IBIWRaycastController raycastController { get; private set; }
    public IBIWGizmosController gizmosController { get; private set; }
    public InitialSceneReferences sceneReferences { get; private set; }


    public void Init(IBIWOutlinerController outlinerController,
        IBIWInputHandler inputHandler,
        IBIWInputWrapper inputWrapper,
        IBIWPublishController publishController,
        IBIWCreatorController creatorController,
        IBIWModeController modeController,
        IBIWFloorHandler floorHandler,
        IBIWEntityHandler entityHandler,
        IBIWActionController actionController,
        IBIWSaveController saveController,
        IBIWRaycastController raycastController,
        IBIWGizmosController gizmosController,
        InitialSceneReferences sceneReferences)
    {
        projectReferencesAsset = Resources.Load<BIWProjectReferences>(PROJECT_REFERENCES_PATH);
        godModeDynamicVariablesAsset = Resources.Load<BIWGodModeDynamicVariables>(GOD_MODE_DYNAMIC_VARIABLE_PATH);
        firstPersonDynamicVariablesAsset = Resources.Load<BIWFirstPersonDynamicVariables>(FIRST_PERSON_DYNAMIC_VARIABLE_PATH);
        inputsReferencesAsset = Resources.Load<BIWInputsReferences>(INPUTS_PATH);

        this.outlinerController = outlinerController;
        this.inputHandler = inputHandler;
        this.inputWrapper = inputWrapper;
        this.publishController = publishController;
        this.creatorController = creatorController;
        this.modeController = modeController;
        this.floorHandler = floorHandler;
        this.entityHandler = entityHandler;
        this.actionController = actionController;
        this.saveController = saveController;
        this.raycastController = raycastController;
        this.gizmosController = gizmosController;
        this.sceneReferences = sceneReferences;
    }

    public void Dispose()
    {
        projectReferencesAsset = null;
        godModeDynamicVariablesAsset = null;
        firstPersonDynamicVariablesAsset = null;
        inputsReferencesAsset = null;
        UnityEngine.Object.Destroy(sceneReferences.gameObject);
    }
}