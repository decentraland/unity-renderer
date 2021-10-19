using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class Context
{
    private const string PROJECT_REFERENCES_PATH = "ScriptableObjects/ProjectReferences";
    private const string INPUTS_PATH = "ScriptableObjects/InputReferences";

    //Scriptable Objects
    public BIWProjectReferences projectReferencesAsset { get; private set; }
    public BIWInputsReferences inputsReferencesAsset { get; private set; }

    //Panel HUD
    public IBuilderProjectsPanelController panelHUD { get; private set; }
    public IBIWEditor editor;

    public InitialSceneReferences.Data sceneReferences { get; private set; }

    //Editor
    public EditorContext editorContext { get; private set; }

    public Context(IBIWEditor editor,
        IBuilderProjectsPanelController panelHUD,
        IBuilderEditorHUDController editorHUD,
        IBIWOutlinerController outlinerController,
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
        InitialSceneReferences.Data sceneReferences)
    {

        projectReferencesAsset = Resources.Load<BIWProjectReferences>(PROJECT_REFERENCES_PATH);
        inputsReferencesAsset = Resources.Load<BIWInputsReferences>(INPUTS_PATH);

        this.sceneReferences = sceneReferences;

        //Builder parts
        this.panelHUD = panelHUD;
        this.editor = editor;

        editorContext = new EditorContext(editorHUD,
            outlinerController,
            inputHandler,
            inputWrapper,
            publishController,
            creatorController,
            modeController,
            floorHandler,
            entityHandler,
            actionController,
            saveController,
            raycastController,
            gizmosController,
            sceneReferences
        );

    }

    public void Dispose()
    {
        editorContext.Dispose();
        panelHUD.Dispose();
        projectReferencesAsset = null;
        inputsReferencesAsset = null;
    }
}

public class EditorContext
{

    private const string GOD_MODE_DYNAMIC_VARIABLE_PATH = "ScriptableObjects/GodModeVariables";
    private const string FIRST_PERSON_DYNAMIC_VARIABLE_PATH = "ScriptableObjects/FirstPersonVariables";

    //Scriptable Objects
    public BIWGodModeDynamicVariables godModeDynamicVariablesAsset { get; private set; }
    public BIWFirstPersonDynamicVariables firstPersonDynamicVariablesAsset { get; private set; }

    //HUD
    public IBuilderEditorHUDController editorHUD { get; private set; }

    // BIW Controllers
    public IBIWOutlinerController outlinerController { get; private set; }
    public IBIWInputHandler inputHandler { get; private set; }
    public IBIWInputWrapper inputWrapper { get; private set; }
    public IBIWPublishController publishController { get; private set; }
    public IBIWCreatorController creatorController { get; private set; }
    public IBIWModeController modeController { get; private set; }
    public IBIWFloorHandler floorHandler { get; internal set; }
    public IBIWEntityHandler entityHandler { get; private set; }
    public IBIWActionController actionController { get; private set; }
    public IBIWSaveController saveController { get; private set; }
    public IBIWRaycastController raycastController { get; private set; }
    public IBIWGizmosController gizmosController { get; private set; }
    public InitialSceneReferences.Data sceneReferences { get; private set; }

    public EditorContext(IBuilderEditorHUDController editorHUD,
        IBIWOutlinerController outlinerController,
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
        InitialSceneReferences.Data sceneReferences)
    {
        godModeDynamicVariablesAsset = Resources.Load<BIWGodModeDynamicVariables>(GOD_MODE_DYNAMIC_VARIABLE_PATH);
        firstPersonDynamicVariablesAsset = Resources.Load<BIWFirstPersonDynamicVariables>(FIRST_PERSON_DYNAMIC_VARIABLE_PATH);

        this.editorHUD = editorHUD;

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
        editorHUD.Dispose();

        outlinerController.Dispose();
        inputHandler.Dispose();
        inputWrapper.Dispose();
        publishController.Dispose();
        creatorController.Dispose();
        modeController.Dispose();
        floorHandler.Dispose();
        entityHandler.Dispose();
        actionController.Dispose();
        saveController.Dispose();
        raycastController.Dispose();
        gizmosController.Dispose();

        godModeDynamicVariablesAsset = null;
        firstPersonDynamicVariablesAsset = null;
    }
}