using System.Collections;
using System.Collections.Generic;
using DCL.Components.Video;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;
using DCL.Components.Video.Plugin;
using DCL.Helpers;
using DCL.Interface;
using DCL.SettingsCommon;

namespace DCL.Components
{
    public class DCLVideoTexture : DCLTexture
    {
#if UNITY_EDITOR
        internal static bool isTest = true;
#else
        internal static bool isTest = false;
#endif

        private const float OUTOFSCENE_TEX_UPDATE_INTERVAL_IN_SECONDS = 1.5f;
        private const float VIDEO_PROGRESS_UPDATE_INTERVAL_IN_SECONDS = 1f;

        [System.Serializable]
        new public class Model : BaseModel
        {
            public string videoClipId;
            public bool playing = false;
            public float volume = 1f;
            public float playbackRate = 1f;
            public bool loop = false;
            public float seek = -1;
            public BabylonWrapMode wrap = BabylonWrapMode.CLAMP;
            public FilterMode samplingMode = FilterMode.Bilinear;

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
        }

        internal IVideoPlayer texturePlayer;
        private Coroutine texturePlayerUpdateRoutine;
        private float baseVolume;
        private float distanceVolumeModifier = 1f;
        private bool isPlayStateDirty = false;
        internal bool isVisible = false;

        private bool isPlayerInScene = true;
        private float currUpdateIntervalTime = OUTOFSCENE_TEX_UPDATE_INTERVAL_IN_SECONDS;
        private float lastVideoProgressReportTime;

        internal Dictionary<string, MaterialInfo> attachedMaterials = new Dictionary<string, MaterialInfo>();
        private string lastVideoClipID;
        private VideoState previousVideoState;

        public DCLVideoTexture()
        {
            model = new Model();

            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange += OnVirtualAudioMixerChangedValue;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDisposed)
                yield break;

            var model = (Model) newModel;

            unitySamplingMode = model.samplingMode;

            switch (model.wrap)
            {
                case BabylonWrapMode.CLAMP:
                    unityWrap = TextureWrapMode.Clamp;
                    break;
                case BabylonWrapMode.WRAP:
                    unityWrap = TextureWrapMode.Repeat;
                    break;
                case BabylonWrapMode.MIRROR:
                    unityWrap = TextureWrapMode.Mirror;
                    break;
            }
            lastVideoClipID = model.videoClipId;

            if (texturePlayer == null)
            {
                DCLVideoClip dclVideoClip = scene.GetSharedComponent(lastVideoClipID) as DCLVideoClip;

                if (dclVideoClip == null)
                {
                    Debug.LogError("Wrong video clip type when playing VideoTexture!!");
                    yield break;
                }

                string videoId = (!string.IsNullOrEmpty(scene.sceneData.id)) ? scene.sceneData.id + id : scene.GetHashCode().ToString() + id;
                texturePlayer = CreateTexturePlayer(videoId, dclVideoClip);
                texturePlayerUpdateRoutine = CoroutineStarter.Start(OnUpdate());
                CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChanged;
                CommonScriptableObjects.sceneID.OnChange += OnSceneIDChanged;
                scene.OnEntityRemoved += OnEntityRemoved;
                Settings.i.generalSettings.OnChanged += OnSettingsChanged;

                OnSceneIDChanged(CommonScriptableObjects.sceneID.Get(), null);
            }

            // NOTE: create texture for testing cause real texture will only be created on web platform
            if (isTest)
            {
                if (texture == null)
                    texture = new Texture2D(1, 1);
            }

            if (texture == null)
            {
                while (texturePlayer.texture == null && !texturePlayer.isError)
                {
                    yield return null;
                }

                if (texturePlayer.isError)
                {
                    if (texturePlayerUpdateRoutine != null)
                    {
                        CoroutineStarter.Stop(texturePlayerUpdateRoutine);
                        texturePlayerUpdateRoutine = null;
                    }

                    yield break;
                }

                texture = texturePlayer.texture;
                isPlayStateDirty = true;
            }

            if (texturePlayer != null)
            {
                if (model.seek >= 0)
                {
                    texturePlayer.SetTime(model.seek);
                    model.seek = -1;

                    // Applying seek is not immediate
                    yield return null;
                }

                if (model.playing)
                {
                    texturePlayer.Play();
                }
                else
                {
                    texturePlayer.Pause();
                }

                ReportVideoProgress();

                if (baseVolume != model.volume)
                {
                    baseVolume = model.volume;
                    UpdateVolume();
                }

                texturePlayer.SetPlaybackRate(model.playbackRate);
                texturePlayer.SetLoop(model.loop);
            }
        }
        protected virtual IVideoPlayer CreateTexturePlayer(string videoId, DCLVideoClip dclVideoClip)
        {
            return new WebVideoPlayer(videoId, dclVideoClip.GetUrl(), dclVideoClip.isStream, new WebVideoPlayerNative());
        }

        public float GetVolume() { return ((Model) model).volume; }

        private bool HasTexturePropertiesChanged() { return texture.wrapMode != unityWrap || texture.filterMode != unitySamplingMode; }

        private void ApplyTextureProperties()
        {
            texture.wrapMode = unityWrap;
            texture.filterMode = unitySamplingMode;
            texture.Compress(false);
            texture.Apply(unitySamplingMode != FilterMode.Point, true);
        }

        private IEnumerator OnUpdate()
        {
            while (true)
            {
                UpdateDirtyState();
                UpdateVideoTexture();
                UpdateProgressReport();
                yield return null;
            }
        }
        private void UpdateDirtyState()
        {
            if (isPlayStateDirty)
            {
                CalculateVideoVolumeAndPlayStatus();
                isPlayStateDirty = false;
            }
        }
        private void UpdateVideoTexture()
        {
            if (!isPlayerInScene && currUpdateIntervalTime < OUTOFSCENE_TEX_UPDATE_INTERVAL_IN_SECONDS)
            {
                currUpdateIntervalTime += Time.unscaledDeltaTime;
            }
            else if (texturePlayer != null && !isTest)
            {
                currUpdateIntervalTime = 0;
                texturePlayer.UpdateWebVideoTexture();
            }
        }
        private void UpdateProgressReport()
        {
            var currentState = texturePlayer.GetState();
            if ( currentState == VideoState.PLAYING 
                 && IsTimeToReportVideoProgress()
                 || previousVideoState != currentState)
            {
                ReportVideoProgress();
            }
        }
        private void ReportVideoProgress()
        {
            lastVideoProgressReportTime = Time.unscaledTime;
            VideoState videoState = texturePlayer.GetState();
            previousVideoState = videoState;
            var videoStatus = (int)videoState;
            var currentOffset = texturePlayer.GetTime();
            var length = texturePlayer.GetDuration();
            WebInterface.ReportVideoProgressEvent(id, scene.sceneData.id, lastVideoClipID, videoStatus, currentOffset, length );
        }
        private bool IsTimeToReportVideoProgress() { return Time.unscaledTime - lastVideoProgressReportTime > VIDEO_PROGRESS_UPDATE_INTERVAL_IN_SECONDS; }

        private void CalculateVideoVolumeAndPlayStatus()
        {
            isVisible = false;
            float minDistance = float.MaxValue;
            distanceVolumeModifier = 0;

            if (attachedMaterials.Count > 0)
            {
                using (var iterator = attachedMaterials.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        var materialInfo = iterator.Current;
                        if (materialInfo.Value.IsVisible())
                        {
                            isVisible = true;
                            var entityDist = materialInfo.Value.GetClosestDistanceSqr(DCLCharacterController.i.transform.position);
                            if (entityDist < minDistance)
                                minDistance = entityDist;
                            // NOTE: if current minDistance is enough for full volume then there is no need to keep iterating to check distances
                            if (minDistance <= DCL.Configuration.ParcelSettings.PARCEL_SIZE * DCL.Configuration.ParcelSettings.PARCEL_SIZE)
                                break;
                        }
                    }
                }
            }

            if (isVisible)
            {
                const float maxDistanceBlockForSound = 6;
                float sqrParcelDistance = DCL.Configuration.ParcelSettings.PARCEL_SIZE * DCL.Configuration.ParcelSettings.PARCEL_SIZE * 2.25f;
                distanceVolumeModifier = 1 - Mathf.Clamp01(Mathf.FloorToInt(minDistance / sqrParcelDistance) / maxDistanceBlockForSound);
            }

            if (texturePlayer != null)
            {
                texturePlayer.visible = isVisible;
            }

            UpdateVolume();
        }

        private void OnVirtualAudioMixerChangedValue(float currentValue, float previousValue) { UpdateVolume(); }

        private void UpdateVolume()
        {
            if (texturePlayer == null)
                return;

            float targetVolume = 0f;

            if (CommonScriptableObjects.rendererState.Get() && IsPlayerInSameSceneAsComponent((CommonScriptableObjects.sceneID.Get())))
            {
                targetVolume = baseVolume * distanceVolumeModifier;
                float virtualMixerVolume = DataStore.i.virtualAudioMixer.sceneSFXVolume.Get();
                float sceneSFXSetting = Settings.i.audioSettings.Data.sceneSFXVolume;
                float masterSetting = Settings.i.audioSettings.Data.masterVolume;
                targetVolume *= Utils.ToVolumeCurve(virtualMixerVolume * sceneSFXSetting * masterSetting);
            }

            texturePlayer.SetVolume(targetVolume);
        }

        private bool IsPlayerInSameSceneAsComponent(string currentSceneId)
        {
            if (scene == null)
                return false;
            if (string.IsNullOrEmpty(currentSceneId))
                return false;

            return (scene.sceneData.id == currentSceneId) || (scene is GlobalScene globalScene && globalScene.isPortableExperience);
        }

        private void OnPlayerCoordsChanged(Vector2Int coords, Vector2Int prevCoords) { isPlayStateDirty = true; }

        private void OnSceneIDChanged(string current, string previous) { isPlayerInScene = IsPlayerInSameSceneAsComponent(current); }

        public override void AttachTo(PBRMaterial material)
        {
            base.AttachTo(material);
            AttachToMaterial(material);
        }

        public override void DetachFrom(PBRMaterial material)
        {
            base.DetachFrom(material);
            DetachFromMaterial(material);
        }

        public override void AttachTo(BasicMaterial material)
        {
            base.AttachTo(material);
            AttachToMaterial(material);
        }

        public override void DetachFrom(BasicMaterial material)
        {
            base.DetachFrom(material);
            DetachFromMaterial(material);
        }

        private void AttachToMaterial(BaseDisposable baseDisposable)
        {
            if (!attachedMaterials.ContainsKey(baseDisposable.id))
            {
                attachedMaterials.Add(baseDisposable.id, new MaterialComponent(baseDisposable));
                baseDisposable.OnAttach += OnEntityAttachedMaterial;
                baseDisposable.OnDetach += OnEntityDetachedMaterial;
                isPlayStateDirty = true;

                if (baseDisposable.attachedEntities.Count > 0)
                {
                    using (var iterator = baseDisposable.attachedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            var entity = iterator.Current;
                            entity.OnShapeUpdated -= OnEntityShapeUpdated;
                            entity.OnShapeUpdated += OnEntityShapeUpdated;
                        }
                    }
                }
            }
        }

        private void DetachFromMaterial(BaseDisposable baseDisposable)
        {
            if (attachedMaterials.ContainsKey(baseDisposable.id))
            {
                attachedMaterials.Remove(baseDisposable.id);
                baseDisposable.OnAttach -= OnEntityAttachedMaterial;
                baseDisposable.OnDetach -= OnEntityDetachedMaterial;
                isPlayStateDirty = true;
            }
        }

        // TODO: we will need an event for visibility change on UI for supporting video
        public override void AttachTo(UIImage image)
        {
            if (!attachedMaterials.ContainsKey(image.id))
            {
                attachedMaterials.Add(image.id, new UIShapeComponent(image));
                isPlayStateDirty = true;
            }
        }

        public override void DetachFrom(UIImage image)
        {
            if (attachedMaterials.ContainsKey(image.id))
            {
                attachedMaterials.Remove(image.id);
                isPlayStateDirty = true;
            }
        }

        void OnEntityRemoved(IDCLEntity entity) { isPlayStateDirty = true; }

        void OnSettingsChanged(GeneralSettings settings) { UpdateVolume(); }

        public override void Dispose()
        {
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange -= OnVirtualAudioMixerChangedValue;
            Settings.i.generalSettings.OnChanged -= OnSettingsChanged;
            CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChanged;
            CommonScriptableObjects.sceneID.OnChange -= OnSceneIDChanged;
            if (scene != null)
                scene.OnEntityRemoved -= OnEntityRemoved;
            if (texturePlayerUpdateRoutine != null)
            {
                CoroutineStarter.Stop(texturePlayerUpdateRoutine);
                texturePlayerUpdateRoutine = null;
            }

            if (texturePlayer != null)
            {
                texturePlayer.Dispose();
                texturePlayer = null;
            }

            Utils.SafeDestroy(texture);
            base.Dispose();
        }

        private void OnEntityAttachedMaterial(IDCLEntity entity) { entity.OnShapeUpdated += OnEntityShapeUpdated; }

        private void OnEntityDetachedMaterial(IDCLEntity entity)
        {
            if (texturePlayer != null)
                texturePlayer.Pause();

            entity.OnShapeUpdated -= OnEntityShapeUpdated;
        }

        private void OnEntityShapeUpdated(IDCLEntity entity) { isPlayStateDirty = true; }

        internal interface MaterialInfo
        {
            float GetClosestDistanceSqr(Vector3 fromPosition);
            bool IsVisible();
        }

        struct MaterialComponent : MaterialInfo
        {
            BaseDisposable component;

            float MaterialInfo.GetClosestDistanceSqr(Vector3 fromPosition)
            {
                float dist = int.MaxValue;
                if (component.attachedEntities.Count > 0)
                {
                    using (var iterator = component.attachedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            var entity = iterator.Current;
                            if (IsEntityVisible(entity))
                            {
                                var entityDist = (entity.meshRootGameObject.transform.position - fromPosition).sqrMagnitude;
                                if (entityDist < dist)
                                    dist = entityDist;
                            }
                        }
                    }
                }

                return dist;
            }

            bool MaterialInfo.IsVisible()
            {
                if (component.attachedEntities.Count > 0)
                {
                    using (var iterator = component.attachedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            if (IsEntityVisible(iterator.Current))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool IsEntityVisible(IDCLEntity entity)
            {
                if (entity.meshesInfo == null)
                    return false;
                if (entity.meshesInfo.currentShape == null)
                    return false;
                return entity.meshesInfo.currentShape.IsVisible();
            }

            public MaterialComponent(BaseDisposable component) { this.component = component; }
        }

        struct UIShapeComponent : MaterialInfo
        {
            UIShape shape;

            float MaterialInfo.GetClosestDistanceSqr(Vector3 fromPosition) { return 0; }

            bool MaterialInfo.IsVisible()
            {
                if (!((UIShape.Model) shape.GetModel()).visible)
                    return false;
                return IsParentVisible(shape);
            }

            bool IsParentVisible(UIShape shape)
            {
                UIShape parent = shape.parentUIComponent;
                if (parent == null)
                    return true;
                if (parent.referencesContainer.canvasGroup.alpha == 0)
                {
                    return false;
                }

                return IsParentVisible(parent);
            }

            public UIShapeComponent(UIShape image) { shape = image; }
        }
    }
}