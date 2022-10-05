using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Tutorial;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Builder;
using UnityEngine;
using Environment = DCL.Environment;

public class BuilderInWorldEditor : IBIWEditor
{
    internal const string AVATAR_HIDE_CONSTRAINT = "BuilderInWorld_Avatar_Invisible";
    
    private GameObject cursorGO;
    private GameObject[] groundVisualsGO;

    internal IBIWOutlinerController outlinerController => context.editorContext.outlinerController;
    internal IBIWInputHandler inputHandler => context.editorContext.inputHandler;
    internal IBIWPublishController publishController => context.editorContext.publishController;
    internal IBIWCreatorController creatorController => context.editorContext.creatorController;
    internal IBIWModeController modeController => context.editorContext.modeController;
    internal IBIWFloorHandler floorHandler => context.editorContext.floorHandler;
    internal IBIWEntityHandler entityHandler => context.editorContext.entityHandler;
    internal IBIWActionController actionController => context.editorContext.actionController;
    internal IBIWSaveController saveController => context.editorContext.saveController;
    internal IBIWInputWrapper inputWrapper => context.editorContext.inputWrapper;
    internal IBIWRaycastController raycastController => context.editorContext.raycastController;
    internal IBIWGizmosController gizmosController => context.editorContext.gizmosController;

    private BuilderInWorldBridge builderInWorldBridge;
    private BuilderInWorldAudioHandler biwAudioHandler;
    private DCL.Camera.CameraController mainCameraController;

    internal IContext context;

    private readonly List<IBIWController> controllers = new List<IBIWController>();
    private Material skyBoxMaterial;

    public bool isBuilderInWorldActivated { get; internal set; } = false;

    private bool isInit = false;
    private Material previousSkyBoxMaterial;
    private PlayerAvatarController avatarRenderer;

    private float startEditorTimeStamp = 0;
    internal IBuilderScene sceneToEdit;

    public void Initialize(IContext context)
    {
        if (isInit)
            return;

        isInit = true;

        this.context = context;

        InitReferences(SceneReferences.i);

        BIWNFTController.i.OnNFTUsageChange += OnNFTUsageChange;

        InitHUD(context);

        InitControllers();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        mainCameraController = context.sceneReferences.cameraController.GetComponent<DCL.Camera.CameraController>();
        biwAudioHandler = UnityEngine.Object.Instantiate(context.projectReferencesAsset.audioPrefab, Vector3.zero, Quaternion.identity).GetComponent<BuilderInWorldAudioHandler>();
        biwAudioHandler.Initialize(context);
        biwAudioHandler.gameObject.SetActive(false);

        floorHandler.OnAllParcelsFloorLoaded += NewSceneFloorLoaded;
        
        avatarRenderer = context.sceneReferences.playerAvatarController.GetComponent<PlayerAvatarController>();
    }

    public void InitReferences(SceneReferences sceneReferences)
    {
        builderInWorldBridge = sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();
        cursorGO = sceneReferences.cursorCanvas;

        List<GameObject> grounds = new List<GameObject>();

        if (sceneReferences.groundVisual != null)
        {
            for (int i = 0; i < sceneReferences.groundVisual.transform.transform.childCount; i++)
            {
                grounds.Add(sceneReferences.groundVisual.transform.transform.GetChild(i).gameObject);
            }
        }

        groundVisualsGO = grounds.ToArray();
        skyBoxMaterial = context.projectReferencesAsset.skyBoxMaterial;
    }

    private void InitHUD(IContext context)
    {
        context.editorContext.editorHUD.Initialize(context);
        context.editorContext.editorHUD.OnTutorialAction += StartTutorial;
        context.editorContext.editorHUD.OnProjectNameAndDescriptionChanged += ChangeProjectNameAndDescription;
    }

    public void Dispose()
    {
        if (context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnProjectNameAndDescriptionChanged -= ChangeProjectNameAndDescription;
            context.editorContext.editorHUD.OnTutorialAction -= StartTutorial;
        }
        
        floorHandler.OnAllParcelsFloorLoaded -= NewSceneFloorLoaded;
        BIWNFTController.i.OnNFTUsageChange -= OnNFTUsageChange;
        BIWNFTController.i.Dispose();

        CleanItems();

        if (biwAudioHandler.gameObject != null)
        {
            biwAudioHandler.Dispose();
            UnityEngine.Object.Destroy(biwAudioHandler.gameObject);
        }
    }

    public void OnGUI()
    {
        if (!isBuilderInWorldActivated)
            return;

        foreach (var controller in controllers)
        {
            controller.OnGUI();
        }
    }

    public void Update()
    {
        if (!isBuilderInWorldActivated)
            return;

        foreach (var controller in controllers)
        {
            controller.Update();
        }
    }

    public void LateUpdate()
    {
        if (!isBuilderInWorldActivated)
            return;

        foreach (var controller in controllers)
        {
            controller.LateUpdate();
        }
    }

    private void OnNFTUsageChange()
    {
        context.editorContext.editorHUD.RefreshCatalogAssetPack();
        context.editorContext.editorHUD.RefreshCatalogContent();
    }

    private void InitControllers()
    {
        InitController(entityHandler);
        InitController(modeController);
        InitController(publishController);
        InitController(creatorController);
        InitController(outlinerController);
        InitController(floorHandler);
        InitController(inputHandler);
        InitController(actionController);
        InitController(inputWrapper);
        InitController(raycastController);
        InitController(gizmosController);
        InitController(saveController);
    }

    public void InitController(IBIWController controller)
    {
        controller.Initialize(context);
        controllers.Add(controller);
    }

    private void StartTutorial() { TutorialController.i.SetBuilderInWorldTutorialEnabled(); }

    public void CleanItems()
    {
        if ( context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.Dispose();

        Camera camera = Camera.main;

        if (camera != null)
        {
            BIWOutline outliner = camera.GetComponent<BIWOutline>();
            UnityEngine.Object.Destroy(outliner);
        }

        floorHandler?.CleanUp();
        creatorController?.CleanUp();
    }

    private void ChangeProjectNameAndDescription(string name, string description)
    {
        sceneToEdit.manifest.project.title = name;
        sceneToEdit.manifest.project.description = description;

        saveController.ForceSave();
    }

    public void EnterEditMode(IBuilderScene builderScene)
    {
        sceneToEdit = builderScene;

        BIWNFTController.i.StartEditMode();
        ParcelSettings.VISUAL_LOADING_ENABLED = false;

        if (biwAudioHandler != null && biwAudioHandler.gameObject != null)
            biwAudioHandler.gameObject.SetActive(true);

        cursorGO.SetActive(false);

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.SetParcelScene(sceneToEdit.scene);
            context.editorContext.editorHUD.RefreshCatalogContent();
            context.editorContext.editorHUD.RefreshCatalogAssetPack();
            context.editorContext.editorHUD.SetVisibilityOfCatalog(true);
            context.editorContext.editorHUD.SetVisibilityOfInspector(true);
            if (sceneToEdit.HasBeenCreatedThisSession() && sceneToEdit.sceneType == IBuilderScene.SceneType.LAND)
                context.editorContext.editorHUD.NewSceneForLand(sceneToEdit);
        }

        var culling = mainCameraController.GetCulling();
        mainCameraController.SetCulling(  BIWUtils.GetBIWCulling(culling));

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);
        DataStore.i.builderInWorld.showTaskBar.Set(true);

        EnterBiwControllers();
        Environment.i.world.sceneController.ActivateBuilderInWorldEditScene();

        if (IsNewScene())
            SetupNewScene();

        isBuilderInWorldActivated = true;

        previousSkyBoxMaterial = RenderSettings.skybox;
        RenderSettings.skybox = skyBoxMaterial;
        
        DeactivateAvatars();

        foreach (var groundVisual in groundVisualsGO)
        {
            groundVisual.SetActive(false);
        }

        DataStore.i.player.otherPlayers.OnAdded += DeactivatePlayerAvatar;
        
        startEditorTimeStamp = Time.realtimeSinceStartup;

        BIWAnalytics.AddSceneInfo(sceneToEdit.scene.sceneData.basePosition, BIWUtils.GetLandOwnershipType(DataStore.i.builderInWorld.landsWithAccess.Get().ToList(), sceneToEdit.scene).ToString(), BIWUtils.GetSceneSize(sceneToEdit.scene));
    }

    public void ExitEditMode()
    {
        Environment.i.platform.cullingController.Start();
        BIWNFTController.i.ExitEditMode();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);
        DataStore.i.builderInWorld.showTaskBar.Set(true);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;

        var culling = mainCameraController.GetCulling();
        culling -= BIWSettings.FX_LAYER;
        mainCameraController.SetCulling(culling);

        outlinerController.CancelAllOutlines();

        cursorGO.SetActive(true);

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.ClearEntityList();
            context.editorContext.editorHUD.SetVisibility(false);
        }

        Environment.i.world.sceneController.DeactivateBuilderInWorldEditScene();
        Environment.i.world.blockersController.SetEnabled(true);

        ExitBiwControllers();

        foreach (var groundVisual in groundVisualsGO)
        {
            groundVisual.SetActive(true);
        }

        isBuilderInWorldActivated = false;
        RenderSettings.skybox = previousSkyBoxMaterial;

        if (biwAudioHandler.gameObject != null)
            biwAudioHandler.gameObject.SetActive(false);
        DataStore.i.common.appMode.Set(AppMode.DEFAULT);
        DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(1f);

        ActivateAvatars();
        
        BIWAnalytics.ExitEditor(Time.realtimeSinceStartup - startEditorTimeStamp);
    }

    internal void ActivateAvatars()
    {
        avatarRenderer.SetAvatarVisibility(true);

        foreach (Player player in DataStore.i.player.otherPlayers.GetValues())
        {
            ActivatePlayerAvatar(null,player);
        }
    }

    internal void ActivatePlayerAvatar(string id,Player player)
    {
        player.avatar.RemoveVisibilityConstrain(AVATAR_HIDE_CONSTRAINT);
        player.playerName.RemoveVisibilityConstaint(AVATAR_HIDE_CONSTRAINT);
    }    
    
    internal void DeactivatePlayerAvatar(string id,Player player)
    {
        player.avatar.AddVisibilityConstraint(AVATAR_HIDE_CONSTRAINT);
        player.playerName.AddVisibilityConstaint(AVATAR_HIDE_CONSTRAINT);
    }

    internal void DeactivateAvatars()
    {
        avatarRenderer.SetAvatarVisibility(false);

        foreach (Player player in DataStore.i.player.otherPlayers.GetValues())
        {
            DeactivatePlayerAvatar(null, player);
        }
    }

    public void EnterBiwControllers()
    {
        foreach (var controller in controllers)
        {
            controller.EnterEditMode(sceneToEdit);
        }

        //Note: This audio should inside the controllers, it is here because it is still a monobehaviour
        biwAudioHandler.EnterEditMode(sceneToEdit.scene);
    }

    public void ExitBiwControllers()
    {
        foreach (var controller in controllers)
        {
            controller.ExitEditMode();
        }

        if (biwAudioHandler.gameObject != null)
            biwAudioHandler.ExitEditMode();
    }

    public bool IsNewScene() { return sceneToEdit.scene.entities.Count <= 0; }

    public void SetupNewScene() { floorHandler.CreateDefaultFloor(); }

    private void NewSceneFloorLoaded()
    {
        if (!sceneToEdit.HasBeenCreatedThisSession())
            return;
        
        context.cameraController.TakeSceneScreenshotFromResetPosition(snapshot =>
            {
                context.builderAPIController.SetThumbnail(sceneToEdit.manifest.project.id, snapshot);
            }
        );
    }
}