using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Builder
{
    internal class SectionPlaceGeneralSettingsController : SectionBase, ISelectPlaceListener, ISectionUpdateSceneDataRequester
    {
        public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionSceneGeneralSettingsView";

        internal const string PERMISSION_MOVE_PLAYER = "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE";
        internal const string PERMISSION_TRIGGER_EMOTES = "ALLOW_TO_TRIGGER_AVATAR_EMOTE";

        private IPlaceData placeData;

        private readonly SceneDataUpdatePayload sceneDataUpdatePayload = new SceneDataUpdatePayload();
        private readonly SectionSceneGeneralSettingsView view;

        public event Action<string, SceneDataUpdatePayload> OnRequestUpdateSceneData;

        public SectionPlaceGeneralSettingsController() : this(
            Object.Instantiate(Resources.Load<SectionSceneGeneralSettingsView>(VIEW_PREFAB_PATH))
        ) { }

        public SectionPlaceGeneralSettingsController(SectionSceneGeneralSettingsView view)
        {
            this.view = view;
            view.OnApplyChanges += OnApplyChanges;
        }

        public override void Dispose()
        {
            view.OnApplyChanges -= OnApplyChanges;
            Object.Destroy(view.gameObject);
        }

        public override void SetViewContainer(Transform viewContainer) { view.SetParent(viewContainer); }

        public void SetSceneData(IPlaceData placeData)
        {
            this.placeData = placeData;

            view.SetName(placeData.name);
            view.SetDescription(placeData.description);
            view.SetConfigurationActive(placeData.isDeployed);
            view.SetPermissionsActive(placeData.isDeployed);

            if (placeData.isDeployed)
            {
                view.SetAllowMovePlayer(placeData.requiredPermissions != null && placeData.requiredPermissions.Contains(PERMISSION_MOVE_PLAYER));
                view.SetAllowTriggerEmotes(placeData.requiredPermissions != null && placeData.requiredPermissions.Contains(PERMISSION_TRIGGER_EMOTES));
                view.SetAllowVoiceChat(placeData.allowVoiceChat);
                view.SetMatureContent(placeData.isMatureContent);
            }
        }

        protected override void OnShow() { view.SetActive(true); }

        protected override void OnHide() { view.SetActive(false); }
        void ISelectPlaceListener.OnSelectScene(IPlaceCardView placeCardView) { SetSceneData(placeCardView.PlaceData); }

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
            OnRequestUpdateSceneData?.Invoke(placeData.id, sceneDataUpdatePayload);
        }
    }
}