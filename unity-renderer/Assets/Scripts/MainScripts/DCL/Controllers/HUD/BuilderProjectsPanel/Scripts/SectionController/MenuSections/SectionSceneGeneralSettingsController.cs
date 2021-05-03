using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionSceneGeneralSettingsController : SectionBase, ISelectSceneListener, ISectionUpdateSceneDataRequester
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionSceneGeneralSettingsView";
    
    internal const string PERMISSION_MOVE_PLAYER = "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE";
    internal const string PERMISSION_TRIGGER_EMOTES = "ALLOW_TO_TRIGGER_AVATAR_EMOTE";

    private ISceneData sceneData;
    
    private readonly SceneDataUpdatePayload sceneDataUpdatePayload = new SceneDataUpdatePayload();
    private readonly SectionSceneGeneralSettingsView view;

    public event Action<string, SceneDataUpdatePayload> OnRequestUpdateSceneData;

    public SectionSceneGeneralSettingsController() : this(
        Object.Instantiate(Resources.Load<SectionSceneGeneralSettingsView>(VIEW_PREFAB_PATH))
    )
    {
    }
    
    public SectionSceneGeneralSettingsController(SectionSceneGeneralSettingsView view)
    {
        this.view = view;
        view.OnApplyChanges += OnApplyChanges;
    }

    public override void Dispose()
    {
        view.OnApplyChanges -= OnApplyChanges;
        Object.Destroy(view.gameObject);
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.SetParent(viewContainer);
    }

    public void SetSceneData(ISceneData sceneData)
    {
        this.sceneData = sceneData;
        
        view.SetName(sceneData.name);
        view.SetDescription(sceneData.description);
        view.SetConfigurationActive(sceneData.isDeployed);
        view.SetPermissionsActive(sceneData.isDeployed);
        
        if (sceneData.isDeployed)
        {
            view.SetAllowMovePlayer(sceneData.requiredPermissions != null && sceneData.requiredPermissions.Contains(PERMISSION_MOVE_PLAYER));
            view.SetAllowTriggerEmotes(sceneData.requiredPermissions != null && sceneData.requiredPermissions.Contains(PERMISSION_TRIGGER_EMOTES));
            view.SetAllowVoiceChat(sceneData.allowVoiceChat);
            view.SetMatureContent(sceneData.isMatureContent);
        }
    }

    protected override void OnShow()
    {
        view.SetActive(true);
    }

    protected override void OnHide()
    {
        view.SetActive(false);
    }
    void ISelectSceneListener.OnSelectScene(ISceneCardView sceneCardView)
    {
        SetSceneData(sceneCardView.sceneData);
    }

    void OnApplyChanges()
    {
        sceneDataUpdatePayload.name = view.GetName();
        sceneDataUpdatePayload.description = view.GetDescription();
        sceneDataUpdatePayload.allowVoiceChat = view.GetAllowVoiceChat();
        sceneDataUpdatePayload.isMatureContent = view.GetMatureContent();

        string[] permissions = null;
        if (view.GetAllowMovePlayer() && view.GetAllowTriggerEmotes())
        {
            permissions = new [] { PERMISSION_MOVE_PLAYER, PERMISSION_TRIGGER_EMOTES };
        }
        else if (view.GetAllowMovePlayer())
        {
            permissions = new [] { PERMISSION_MOVE_PLAYER };
        }
        else if (view.GetAllowTriggerEmotes())
        {
            permissions = new [] { PERMISSION_TRIGGER_EMOTES };
        }
        sceneDataUpdatePayload.requiredPermissions = permissions;
        OnRequestUpdateSceneData?.Invoke(sceneData.id, sceneDataUpdatePayload);
    }
}
