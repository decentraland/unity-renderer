using DCL.CameraTool;
using DCL.SettingsCommon;
using System;
using System.Collections.Generic;
using UnityEngine;
using QualitySettings = DCL.SettingsCommon.QualitySettings;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        private const float RENDERED_DOT_PRODUCT_ANGLE = 0.25f;
        private const float AVATARS_INVISIBILITY_DISTANCE = 1.75f;
        private const float MIN_DISTANCE_BETWEEN_NAMES_PIXELS = 70f;
        private const float MIN_ANIMATION_FRAME_SKIP_DISTANCE_MULTIPLIER = 0.3f;
        private const float MAX_ANIMATION_FRAME_SKIP_DISTANCE_MULTIPLIER = 2f;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private BaseVariable<float> simpleAvatarDistance => DataStore.i.avatarsLOD.simpleAvatarDistance;
        private BaseVariable<int> maxAvatars => DataStore.i.avatarsLOD.maxAvatars;
        private BaseVariable<int> maxImpostors => DataStore.i.avatarsLOD.maxImpostors;
        private QualitySettings qualitySettings => Settings.i.qualitySettings.Data;

        private readonly GPUSkinningThrottlingCurveSO gpuSkinningThrottlingCurve;
        private readonly SimpleOverlappingTracker overlappingTracker = new (MIN_DISTANCE_BETWEEN_NAMES_PIXELS);

        private UnityEngine.Camera camera;
        internal Transform cameraTransformValue;

        private readonly List<(IAvatarLODController lodController, float distance)> lodControllersWithDistance = new ();
        public Dictionary<string, IAvatarLODController> LodControllers { get; } = new ();

        private Transform cameraTransform
        {
            get
            {
                GetMainCamera();
                return cameraTransformValue;
            }
        }

        private Vector3 cameraTransformPosition => cameraTransform != null ? cameraTransform.position : Vector3.zero;
        private Vector3 cameraTransformForward => cameraTransform != null ? cameraTransform.forward : Vector3.forward;

        public AvatarsLODController()
        {
            gpuSkinningThrottlingCurve = Resources.Load<GPUSkinningThrottlingCurveSO>("GPUSkinningThrottlingCurve");
        }

        public void Initialize()
        {
            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);

            foreach (IAvatarLODController lodController in LodControllers.Values) { lodController.Dispose(); }

            LodControllers.Clear();

            foreach (var keyValuePair in otherPlayers.Get()) { RegisterAvatar(keyValuePair.Key, keyValuePair.Value); }

            otherPlayers.OnAdded += RegisterAvatar;
            otherPlayers.OnRemoved += UnregisterAvatar;
        }

        public void SetCamera(UnityEngine.Camera newCamera)
        {
            camera = newCamera;
            cameraTransformValue = camera.transform;
        }

        public void RegisterAvatar(string id, Player player)
        {
            if (LodControllers.ContainsKey(id))
                return;

            LodControllers.Add(id, CreateLodController(player));
        }

        protected internal virtual IAvatarLODController CreateLodController(Player player) =>
            new AvatarLODController(player);

        public void UnregisterAvatar(string id, Player player)
        {
            if (!LodControllers.ContainsKey(id))
                return;

            LodControllers[id].SetLOD0();
            LodControllers[id].Dispose();
            LodControllers.Remove(id);
        }

        public void Update()
        {
            UpdateAllLODs(maxAvatars.Get(), maxImpostors.Get());
        }

        internal void UpdateAllLODs(int maxAvatars = DataStore_AvatarsLOD.DEFAULT_MAX_AVATAR, int maxImpostors = DataStore_AvatarsLOD.DEFAULT_MAX_IMPOSTORS)
        {
            GetMainCamera();

            var avatarsCount = 0; //Full Avatar + Simple Avatar
            var impostorCount = 0; //Impostor

            float simpleAvatarDistance = this.simpleAvatarDistance.Get();
            Vector3 ownPlayerPosition = CommonScriptableObjects.playerUnityPosition.Get();

            overlappingTracker.Reset();

            foreach (var controllerDistancePair in
                     ComposeLODControllersSortedByDistance(LodControllers.Values, ownPlayerPosition))
            {
                (IAvatarLODController lodController, float distance) = controllerDistancePair;

                if (IsInInvisibleDistance(distance))
                {
                    lodController.SetNameVisible(false);
                    lodController.SetInvisible();
                    continue;
                }

                if (avatarsCount < maxAvatars)
                {
                    lodController.SetAnimationThrottling(CalculateAnimationTrhottle(distance));

                    if (distance < simpleAvatarDistance)
                        lodController.SetLOD0();
                    else
                        lodController.SetLOD1();

                    avatarsCount++;

                    lodController.SetNameVisible(camera == null || overlappingTracker.RegisterPosition(lodController.player.playerName.ScreenSpacePos(camera)));

                    continue;
                }

                lodController.SetNameVisible(false);

                if (impostorCount < maxImpostors)
                {
                    lodController.SetLOD2();
                    lodController.UpdateImpostorTint(distance);
                    impostorCount++;
                    continue;
                }

                lodController.SetInvisible();
            }
        }

        private int CalculateAnimationTrhottle(float distance)
        {
            if (!qualitySettings.enableDetailObjectCulling) return 1;

            float culling = qualitySettings.detailObjectCullingLimit;

            // The higher the culling, the higher distance multiplier we give to the calculation
            float distanceNerf = Mathf.Lerp(
                MIN_ANIMATION_FRAME_SKIP_DISTANCE_MULTIPLIER,
                MAX_ANIMATION_FRAME_SKIP_DISTANCE_MULTIPLIER,
                culling / 100f);

            // The curve starts adding frame skips at 15 distance (one parcel)
            return (int)gpuSkinningThrottlingCurve.curve.Evaluate(distance * distanceNerf);
        }

        private void GetMainCamera()
        {
            if (camera == null)
            {
                camera = UnityEngine.Camera.main;

                if (camera != null)
                    cameraTransformValue = camera.transform;
            }
        }

        private static bool IsInInvisibleDistance(float distance)
        {
            bool firstPersonCamera = CommonScriptableObjects.cameraMode.Get() == CameraMode.ModeId.FirstPerson;

            return firstPersonCamera ? distance < AVATARS_INVISIBILITY_DISTANCE : distance < 0f; // < 0 is behind camera
        }

        private List<(IAvatarLODController lodController, float distance)> ComposeLODControllersSortedByDistance(Dictionary<string, IAvatarLODController>.ValueCollection lodControllers, Vector3 ownPlayerPosition)
        {
            lodControllersWithDistance.Clear();

            foreach (IAvatarLODController x in lodControllers)
                lodControllersWithDistance.Add((x, DistanceToOwnPlayer(x.player, ownPlayerPosition)));

            lodControllersWithDistance.Sort(CompareByDistance());

            return lodControllersWithDistance;

            Comparison<(IAvatarLODController lodController, float distance)> CompareByDistance() =>
                (x, y) => x.distance.CompareTo(y.distance);
        }

        /// <summary>
        /// Returns -1 if player is not in front of camera or not found
        /// </summary>
        private float DistanceToOwnPlayer(Player player, Vector3 ownPlayerPosition)
        {
            if (player == null || !IsInFrontOfCamera(player.worldPosition))
                return -1;

            return Vector3.Distance(ownPlayerPosition, player.worldPosition);
        }

        private bool IsInFrontOfCamera(Vector3 position) =>
            Vector3.Dot(cameraTransformForward, (position - cameraTransformPosition).normalized) >= RENDERED_DOT_PRODUCT_ANGLE;

        public void Dispose()
        {
            foreach (IAvatarLODController lodController in LodControllers.Values) { lodController.Dispose(); }

            otherPlayers.OnAdded -= RegisterAvatar;
            otherPlayers.OnRemoved -= UnregisterAvatar;
            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        }
    }
}
