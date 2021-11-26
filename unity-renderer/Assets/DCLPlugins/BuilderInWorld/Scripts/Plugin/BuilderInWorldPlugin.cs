using DCL;
using DCL.Builder;

public class BuilderInWorldPlugin : PluginFeature
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
    }

    public BuilderInWorldPlugin(IContext context) { this.context = context; }

    public override void Initialize()
    {
        base.Initialize();

        //We init the lands so we don't have a null reference
        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);

        BIWCatalogManager.Init();

        panelController.Initialize(context);
        editor.Initialize(context);
        builderAPIController.Initialize(context);
        sceneManager.Initialize(context);
        cameraController.Initialize(context);

        DataStore.i.builderInWorld.isInitialized.Set(true);
    }

    public override void Dispose()
    {
        base.Dispose();

        editor.Dispose();
        panelController.Dispose();
        sceneManager.Dispose();
        cameraController.Dispose();
        context.Dispose();
    }

    public override void Update()
    {
        base.Update();
        editor.Update();
        sceneManager.Update();
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