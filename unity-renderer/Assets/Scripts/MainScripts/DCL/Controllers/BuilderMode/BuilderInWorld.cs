using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class BuilderInWorld : PluginFeature
{
    private const string DEV_FLAG_NAME = "builder-dev";
    internal readonly IBIWEditor editor;
    internal readonly IBuilderProjectsPanelController panelController;

    internal readonly Context context;

    public BuilderInWorld()
    {
        if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(DEV_FLAG_NAME))
            DataStore.i.builderInWorld.isDevBuild.Set(true);

        panelController = new BuilderProjectsPanelController();
        editor = new BuilderInWorldEditor();

        context = new Context(editor,
            panelController,
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
            InitialSceneReferences.i.data);

        HUDController.i.taskbarHud.SetBuilderInWorldStatus(true);
    }

    public override void Initialize()
    {
        base.Initialize();

        //We init the lands so we don't have a null reference
        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);

        BIWCatalogManager.Init();

        panelController.Initialize();
        editor.Initialize(context);
    }

    public override void Dispose()
    {
        base.Dispose();

        editor.Dispose();
        panelController.Dispose();
        context.Dispose();
    }

    public override void Update()
    {
        base.Update();
        editor.Update();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        editor.LateUpdate();
    }

    public override void OnGUI()
    {
        base.OnGUI();
        editor.OnGUI();
    }
}