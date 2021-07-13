using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWContext
{
    private const string PROJECT_REFERENCES_PATH = "ScriptableObjects/ProjectReferences";
    private const string GOD_MODE_DYNAMIC_VARIABLE_PATH = "ScriptableObjects/GodModeVariables";
    private const string FIRST_PERSON_DYNAMIC_VARIABLE_PATH = "ScriptableObjects/FirstPersonVariables";
    private const string INPUTS_PATH = "ScriptableObjects/InputReferences";

    //Scriptable Objects
    public BIWProjectReferences projectReferences => projectReferencesValue;
    public BIWGodModeDynamicVariables godModeDynamicVariables => godModeDynamicVariablesValue;
    public BIWFirstPersonDynamicVariables firstPersonDynamicVariables => firstPersonDynamicVariablesValue;
    public BIWInputsReferences inputsReferences => inputsReferencesValue;

    private BIWProjectReferences projectReferencesValue;
    private BIWGodModeDynamicVariables godModeDynamicVariablesValue;
    private BIWFirstPersonDynamicVariables firstPersonDynamicVariablesValue;
    private BIWInputsReferences inputsReferencesValue;

    // BIW Controllers
    public IBIWOutlinerController outlinerController => outlinerControllerValue;
    public IBIWInputHandler inputHandler => inputHandlerValue;
    public IBIWInputWrapper inputWrapper => inputWrapperValue;
    public IBIWPublishController publishController => publishControllerValue;
    public IBIWCreatorController creatorController => creatorControllerValue;
    public IBIWModeController modeController => modeControllerValue;
    public IBIWFloorHandler floorHandler => floorHandlerValue;
    public IBIWEntityHandler entityHandler => entityHandlerValue;
    public IBIWActionController actionController => actionControllerValue;
    public IBIWSaveController saveController => saveControllerValue;

    private IBIWOutlinerController outlinerControllerValue;
    private IBIWInputHandler inputHandlerValue;
    private IBIWInputWrapper inputWrapperValue;
    private IBIWPublishController publishControllerValue;
    private IBIWCreatorController creatorControllerValue;
    private IBIWModeController modeControllerValue;
    private IBIWFloorHandler floorHandlerValue;
    private IBIWEntityHandler entityHandlerValue;
    private IBIWActionController actionControllerValue;
    private IBIWSaveController saveControllerValue;

    public void Init(IBIWOutlinerController outlinerController,
        IBIWInputHandler bIWInputHandler,
        IBIWInputWrapper biwInputWrapper,
        IBIWPublishController biwPublishController,
        IBIWCreatorController biwCreatorController,
        IBIWModeController biwModeController,
        IBIWFloorHandler biwFloorHandler,
        IBIWEntityHandler biwEntityHandler,
        IBIWActionController ibiwActionController,
        IBIWSaveController biwSaveController)
    {
        projectReferencesValue = Resources.Load<BIWProjectReferences>(PROJECT_REFERENCES_PATH);
        godModeDynamicVariablesValue = Resources.Load<BIWGodModeDynamicVariables>(GOD_MODE_DYNAMIC_VARIABLE_PATH);
        firstPersonDynamicVariablesValue = Resources.Load<BIWFirstPersonDynamicVariables>(FIRST_PERSON_DYNAMIC_VARIABLE_PATH);
        inputsReferencesValue = Resources.Load<BIWInputsReferences>(INPUTS_PATH);

        outlinerControllerValue = outlinerController;
        inputHandlerValue = bIWInputHandler;
        inputWrapperValue = biwInputWrapper;
        publishControllerValue = biwPublishController;
        creatorControllerValue = biwCreatorController;
        modeControllerValue = biwModeController;
        floorHandlerValue = biwFloorHandler;
        entityHandlerValue = biwEntityHandler;
        actionControllerValue = ibiwActionController;
        saveControllerValue = biwSaveController;
    }

    public void Dispose()
    {
        projectReferencesValue = null;
        godModeDynamicVariablesValue = null;
        firstPersonDynamicVariablesValue = null;
        inputsReferencesValue = null;
    }

}