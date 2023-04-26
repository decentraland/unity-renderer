using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;
using DCL.Components.Video.Plugin;
using DCL.Helpers;
using DCL.Interface;
using DCL.SettingsCommon;
using UnityEngine.Assertions;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCL.Components
{
    public class DCLVideoTexture : DCLTexture
    {
        public static bool VERBOSE = false;
        public static Logger logger = new Logger("DCLVideoTexture") { verboseEnabled = VERBOSE };

        private const float OUTOFSCENE_TEX_UPDATE_INTERVAL_IN_SECONDS = 1.5f;
        private const float VIDEO_PROGRESS_UPDATE_INTERVAL_IN_SECONDS = 1f;

        public static System.Func<IVideoPluginWrapper> videoPluginWrapperBuilder = () => new VideoPluginWrapper_WebGL();

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

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        internal WebVideoPlayer texturePlayer;
        private Coroutine texturePlayerUpdateRoutine;
        private float baseVolume;
        private float distanceVolumeModifier = 1f;
        private bool isPlayStateDirty = false;
        internal bool isVisible = false;

        private bool isInitialized = false;
        private bool isPlayerInScene = true;
        private float currUpdateIntervalTime = OUTOFSCENE_TEX_UPDATE_INTERVAL_IN_SECONDS;
        private float lastVideoProgressReportTime;

        internal Dictionary<string, ISharedComponent> attachedMaterials = new Dictionary<string, ISharedComponent>();
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
            if (isDisposed) { yield break; }

            var model = (Model)newModel;

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
                DCLVideoClip dclVideoClip = scene.componentsManagerLegacy.GetSceneSharedComponent(lastVideoClipID) as DCLVideoClip;

                if (dclVideoClip == null)
                {
                    logger.Error("Wrong video clip type when playing VideoTexture!!");
                    yield break;
                }

                Initialize(dclVideoClip);
            }

            if (texture == null)
            {
                yield return new WaitUntil(() => texturePlayer == null || (texturePlayer.texture != null && texturePlayer.isReady) || texturePlayer.isError);

                if (texturePlayer == null || texturePlayer.isError)
                {
                    if (texturePlayerUpdateRoutine == null) yield break;

                    CoroutineStarter.Stop(texturePlayerUpdateRoutine);
                    texturePlayerUpdateRoutine = null;

                    yield break;
                }

                texture = texturePlayer.texture;
                SetPlayStateDirty();
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

                if (model.playing) { texturePlayer.Play(); }
                else { texturePlayer.Pause(); }

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

        private void Initialize(DCLVideoClip dclVideoClip)
        {
            if (isInitialized) return;
            isInitialized = true;

            string videoId = scene.sceneData.sceneNumber > 0 ? scene.sceneData.sceneNumber + id : scene.GetHashCode().ToString() + id;
            texturePlayer = new WebVideoPlayer(videoId, dclVideoClip.GetUrl(), dclVideoClip.isStream, videoPluginWrapperBuilder.Invoke());
            texturePlayerUpdateRoutine = CoroutineStarter.Start(OnUpdate());

            CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChanged;
            CommonScriptableObjects.sceneNumber.OnChange += OnSceneNumberChanged;
            scene.OnEntityRemoved += SetPlayStateDirty;
            Settings.i.audioSettings.OnChanged += OnAudioSettingsChanged;

            OnSceneNumberChanged(CommonScriptableObjects.sceneNumber.Get(), -1);
        }

        public float GetVolume()
        {
            return ((Model)model).volume;
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
            if (!isPlayStateDirty)
                return;

            CalculateVideoVolumeAndPlayStatus();
            isPlayStateDirty = false;
        }

        private void UpdateVideoTexture()
        {
            if (!isPlayerInScene && currUpdateIntervalTime < OUTOFSCENE_TEX_UPDATE_INTERVAL_IN_SECONDS) { currUpdateIntervalTime += Time.unscaledDeltaTime; }
            else if (texturePlayer != null)
            {
                currUpdateIntervalTime = 0;
                texturePlayer.Update();
                texture = texturePlayer.texture;
            }
        }

        private void UpdateProgressReport()
        {
            var currentState = texturePlayer.GetState();

            if (currentState == VideoState.PLAYING
                && IsTimeToReportVideoProgress()
                || previousVideoState != currentState) { ReportVideoProgress(); }
        }

        private void ReportVideoProgress()
        {
            lastVideoProgressReportTime = Time.unscaledTime;
            VideoState videoState = texturePlayer.GetState();
            previousVideoState = videoState;
            var videoStatus = (int)videoState;
            var currentOffset = texturePlayer.GetTime();
            var length = texturePlayer.GetDuration();
            WebInterface.ReportVideoProgressEvent(id, scene.sceneData.sceneNumber, lastVideoClipID, videoStatus, currentOffset, length);
        }

        private bool IsTimeToReportVideoProgress()
        {
            return Time.unscaledTime - lastVideoProgressReportTime > VIDEO_PROGRESS_UPDATE_INTERVAL_IN_SECONDS;
        }

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
                        bool isComponentVisible = DCLVideoTextureUtils.IsComponentVisible(materialInfo.Value);

                        if (isComponentVisible)
                        {
                            isVisible = true;

                            var entityDist = DCLVideoTextureUtils.GetClosestDistanceSqr(materialInfo.Value,
                                CommonScriptableObjects.playerUnityPosition);

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
                const float maxDistanceBlockForSound = 12;
                float sqrParcelDistance = DCL.Configuration.ParcelSettings.PARCEL_SIZE * DCL.Configuration.ParcelSettings.PARCEL_SIZE * 2.25f;
                distanceVolumeModifier = 1 - Mathf.Clamp01(Mathf.FloorToInt(minDistance / sqrParcelDistance) / maxDistanceBlockForSound);
            }

            if (texturePlayer != null) { texturePlayer.visible = isVisible; }

            UpdateVolume();
        }

        private void OnVirtualAudioMixerChangedValue(float currentValue, float previousValue)
        {
            UpdateVolume();
        }

        private void UpdateVolume()
        {
            if (texturePlayer == null)
                return;

            float targetVolume = 0f;

            if (CommonScriptableObjects.rendererState.Get() && IsPlayerInSameSceneAsComponent(CommonScriptableObjects.sceneNumber.Get()))
            {
                targetVolume = baseVolume * distanceVolumeModifier;
                float virtualMixerVolume = DataStore.i.virtualAudioMixer.sceneSFXVolume.Get();
                float sceneSFXSetting = Settings.i.audioSettings.Data.sceneSFXVolume;
                float masterSetting = Settings.i.audioSettings.Data.masterVolume;
                targetVolume *= Utils.ToVolumeCurve(virtualMixerVolume * sceneSFXSetting * masterSetting);
            }

            texturePlayer.SetVolume(targetVolume);
        }

        private bool IsPlayerInSameSceneAsComponent(int currentSceneNumber)
        {
            if (scene == null)
                return false;

            if (currentSceneNumber <= 0)
                return false;

            return (scene.sceneData.sceneNumber == currentSceneNumber) || (scene.isPersistent);
        }

        private void OnPlayerCoordsChanged(Vector2Int coords, Vector2Int prevCoords) =>
            SetPlayStateDirty();

        private void OnSceneNumberChanged(int current, int previous) =>
            isPlayerInScene = IsPlayerInSameSceneAsComponent(current);

        public override void AttachTo(ISharedComponent component)
        {
            Assert.IsTrue(component != null, "Attachment must not be null!");

            if (attachedMaterials.ContainsKey(component.id))
                return;

            AddReference(component);

            SetPlayStateDirty();
            attachedMaterials.Add(component.id, component);

            component.OnAttach += SetPlayStateDirty;
            component.OnDetach += SetPlayStateDirty;
            DCLVideoTextureUtils.SubscribeToEntityUpdates(component, SetPlayStateDirty);
        }

        public override void DetachFrom(ISharedComponent component)
        {
            Assert.IsTrue(component != null, "Component must not be null!");

            if (!attachedMaterials.ContainsKey(component.id))
                return;

            if (texturePlayer != null)
                texturePlayer.Pause();

            attachedMaterials.Remove(component.id);

            component.OnAttach -= SetPlayStateDirty;
            component.OnDetach -= SetPlayStateDirty;
            DCLVideoTextureUtils.UnsubscribeToEntityShapeUpdate(component, SetPlayStateDirty);

            SetPlayStateDirty();

            if (RemoveReference(component))
            {
                if (attachedEntitiesByComponent.Count == 0)
                    DisposeTexture();
            }
        }

        private void SetPlayStateDirty(IDCLEntity entity = null) =>
            isPlayStateDirty = true;

        private void OnAudioSettingsChanged(AudioSettings settings) =>
            UpdateVolume();

        public override void Dispose()
        {
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange -= OnVirtualAudioMixerChangedValue;
            base.Dispose();
        }

        protected override void DisposeTexture()
        {
            isInitialized = false;
            textureDisposed = true;

            CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChanged;
            CommonScriptableObjects.sceneNumber.OnChange -= OnSceneNumberChanged;
            Settings.i.audioSettings.OnChanged -= OnAudioSettingsChanged;

            if (scene != null)
                scene.OnEntityRemoved -= SetPlayStateDirty;

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
        }
    }
}
