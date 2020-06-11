using DCL.Components;
using DCL.Interface;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class AvatarShape : BaseComponent
    {
        private const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";

        public AvatarName avatarName;
        public AvatarRenderer avatarRenderer;
        public AvatarMovementController avatarMovementController;
        [SerializeField] internal GameObject minimapRepresentation;
        [SerializeField] private AvatarOnPointerDown onPointerDown;
        private StringVariable currentPlayerInfoCardId;

        private string currentSerialization = "";
        public AvatarModel model = new AvatarModel();

        public bool everythingIsLoaded;

        private Vector3? lastAvatarPosition = null;
        private MinimapMetadata.MinimapUserInfo avatarUserInfo = new MinimapMetadata.MinimapUserInfo();

        void Awake()
        {
            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);

            if (string.IsNullOrEmpty(currentSerialization))
                SetMinimapRepresentationActive(false);

            onPointerDown.OnPointerDownReport += PlayerClicked;
        }

        void Start()
        {
            onPointerDown.Setup(scene, entity, new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                button = WebInterface.ACTION_BUTTON.POINTER.ToString(),
                hoverText = "view profile"
            });
        }

        private void PlayerClicked()
        {
            currentPlayerInfoCardId.Set(model?.id);
        }

        void OnDestroy()
        {
            onPointerDown.OnPointerDownReport -= PlayerClicked;
            if (entity != null)
            {
                entity.OnTransformChange = null;
                avatarUserInfo.userId = model.id;
                MinimapMetadataController.i?.UpdateMinimapUserInformation(avatarUserInfo, true);
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): Horrible fix to the double ApplyChanges call, as its breaking the needed logic.
            if (newJson == "{}")
                yield break;

            if (entity != null && entity.OnTransformChange == null)
            {
                entity.OnTransformChange += avatarMovementController.OnTransformChanged;
                entity.OnTransformChange += OnEntityTransformChanged;
            }

            if (currentSerialization == newJson)
                yield break;

            model = SceneController.i.SafeFromJson<AvatarModel>(newJson);

            everythingIsLoaded = false;

            bool avatarDone = false;
            bool avatarFailed = false;

            yield return null; //NOTE(Brian): just in case we have a Object.Destroy waiting to be resolved.

            avatarRenderer.ApplyModel(model, () => avatarDone = true, () => avatarFailed = true);

            yield return new WaitUntil(() => avatarDone || avatarFailed);

            avatarName.SetName(model.name);
            SetMinimapRepresentationActive(true);
            everythingIsLoaded = true;

            avatarUserInfo.userId = model.id;
            avatarUserInfo.userName = model.name;
            avatarUserInfo.worldPosition = lastAvatarPosition != null ? lastAvatarPosition.Value : minimapRepresentation.transform.position;
            UpdateAvatarIconInMinimap(avatarUserInfo);
        }

        void SetMinimapRepresentationActive(bool active)
        {
            if (minimapRepresentation == null)
                return;

            minimapRepresentation.SetActive(active);
        }

        private void OnEntityTransformChanged(DCLTransform.Model updatedModel)
        {
            lastAvatarPosition = updatedModel.position;

            avatarUserInfo.userId = model.id;
            avatarUserInfo.userName = model.name;
            avatarUserInfo.worldPosition = updatedModel.position;
            UpdateAvatarIconInMinimap(avatarUserInfo);
        }

        private void UpdateAvatarIconInMinimap(MinimapMetadata.MinimapUserInfo userInfo)
        {
            MinimapMetadataController.i?.UpdateMinimapUserInformation(new MinimapMetadata.MinimapUserInfo
            {
                userId = userInfo.userId,
                userName = userInfo.userName,
                worldPosition = userInfo.worldPosition
            });
        }
    }
}
