using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using UnityEngine;

public class BuilderInWorldPlugin : IPlugin
{
    private const string DEV_FLAG_NAME = "builder-dev";
    internal IBIWEditor editor;
    internal IBuilderMainPanelController panelController;
    internal IBuilderAPIController builderAPIController;
    internal ISceneManager sceneManager;
    internal ICameraController cameraController;

    internal IContext context;

    public BuilderInWorldPlugin()
    {
        if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(DEV_FLAG_NAME))
            DataStore.i.builderInWorld.isDevBuild.Set(true);

        panelController = new BuilderMainPanelController();
        editor = new BuilderInWorldEditor();
        builderAPIController = new BuilderAPIController();
        sceneManager = new SceneManager();
        cameraController = new CameraController();

        context = new Context(editor,
            panelController,
            builderAPIController,
            sceneManager,
            cameraController,
            new BuilderEditorHUDController(),
            new BIWOutlinerController(),
            new BIWInputHandler(),
            new BIWInputWrapper(),
            new BIWPublishController(),
            new BIWCreatorController(),
            new BIWModeController(),
            new BIWFloorHandler(),
            new BIWEntityHandler(),
            new BIWActionController(),
            new BIWSaveController(),
            new BIWRaycastController(),
            new BIWGizmosController(),
            SceneReferences.i);

        Initialize();
    }

    public BuilderInWorldPlugin(IContext context)
    {
        this.context = context;
        panelController = context.panelHUD;
        editor = context.editor;
        builderAPIController = context.builderAPIController;

        Initialize();
    }

    private void Initialize()
    {
        //We init the lands so we don't have a null reference
        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);

        BIWCatalogManager.Init();

        panelController.Initialize(context);
        editor.Initialize(context);
        builderAPIController.Initialize(context);
        sceneManager.Initialize(context);
        cameraController.Initialize(context);

        if (HUDController.i == null)
            return;

        if (HUDController.i.taskbarHud != null)
            HUDController.i.taskbarHud.SetBuilderInWorldStatus(true);
        else
            HUDController.i.OnTaskbarCreation += TaskBarCreated;

        DCL.Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        DCL.Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        DCL.Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.OnGui, OnGUI);
    }


    private void TaskBarCreated()
    {
        HUDController.i.OnTaskbarCreation -= TaskBarCreated;
        HUDController.i.taskbarHud.SetBuilderInWorldStatus(true);
    }

    public void Dispose()
    {
        if (HUDController.i != null)
            HUDController.i.OnTaskbarCreation -= TaskBarCreated;

        editor.Dispose();
        panelController.Dispose();
        sceneManager.Dispose();
        cameraController.Dispose();
        context.Dispose();

        DCL.Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        DCL.Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        DCL.Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.OnGui, OnGUI);
    }

    public void Update()
    {
        editor.Update();
        sceneManager.Update();
    }

    public void LateUpdate()
    {
        editor.LateUpdate();
    }

    public void OnGUI()
    {
        editor.OnGUI();
    }
}