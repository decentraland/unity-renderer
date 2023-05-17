using DCL.Helpers;
using System.Collections;
using DCL.Controllers;
using UnityEngine;
using DCL.Models;
using DCL.SettingsCommon;
using AudioSettings = DCL.SettingsCommon.AudioSettings;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class DCLAudioSource : BaseComponent, IOutOfSceneBoundariesHandler
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public string audioClipId;
            public bool playing;
            public float volume = 1f;
            public bool loop;
            public float pitch = 1f;
            public long playedAtTimestamp;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.AudioSource)
                    return Utils.SafeUnimplemented<DCLAudioSource, Model>(expected: ComponentBodyPayload.PayloadOneofCase.AudioSource, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.AudioSource.HasLoop) pb.loop = pbModel.AudioSource.Loop;
                if (pbModel.AudioSource.HasPitch) pb.pitch = pbModel.AudioSource.Pitch;
                if (pbModel.AudioSource.HasPlaying) pb.playing = pbModel.AudioSource.Playing;
                if (pbModel.AudioSource.HasVolume) pb.volume = pbModel.AudioSource.Volume;
                if (pbModel.AudioSource.HasAudioClipId) pb.audioClipId = pbModel.AudioSource.AudioClipId;
                if (pbModel.AudioSource.HasPlayedAtTimestamp) pb.playedAtTimestamp = pbModel.AudioSource.PlayedAtTimestamp;

                return pb;
            }
        }

        public float playTime => audioSource.time;
        internal AudioSource audioSource;
        DCLAudioClip lastDCLAudioClip;

        private bool isDestroyed = false;
        public long playedAtTimestamp = 0;
        private bool isOutOfBoundaries = false;

        public override string componentName => "AudioSource";

        private void Awake()
        {
            audioSource = gameObject.GetOrCreateComponent<AudioSource>();
            model = new Model();

            if (Settings.i != null)
                Settings.i.audioSettings.OnChanged += OnAudioSettingsChanged;

            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange += OnVirtualAudioMixerChangedValue;
        }

        public override void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            base.Initialize(scene, entity);
            isOutOfBoundaries = !entity.isInsideSceneBoundaries;
            DataStore.i.sceneBoundariesChecker.Add(entity,this);
        }

        public void InitDCLAudioClip(DCLAudioClip dclAudioClip)
        {
            if (lastDCLAudioClip != null)
            {
                lastDCLAudioClip.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
            }

            lastDCLAudioClip = dclAudioClip;
        }

        public double volume => ((Model) model).volume;

        public override IEnumerator ApplyChanges(BaseModel baseModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy an audiosource before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDestroyed)
                yield break;

            CommonScriptableObjects.sceneNumber.OnChange -= OnCurrentSceneChanged;
            CommonScriptableObjects.sceneNumber.OnChange += OnCurrentSceneChanged;

            ApplyCurrentModel();

            yield return null;
        }

        private void ApplyCurrentModel()
        {
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource is null!.");
                return;
            }

            Model model = (Model) this.model;
            UpdateAudioSourceVolume();
            audioSource.loop = model.loop;
            audioSource.pitch = model.pitch;
            audioSource.spatialBlend = 1;
            audioSource.dopplerLevel = 0.1f;

            if (model.playing)
            {
                DCLAudioClip dclAudioClip = scene.componentsManagerLegacy.GetSceneSharedComponent(model.audioClipId) as DCLAudioClip;

                if (dclAudioClip != null)
                {
                    InitDCLAudioClip(dclAudioClip);
                    //NOTE(Brian): Play if finished loading, otherwise will wait for the loading to complete (or fail).
                    if (dclAudioClip.loadingState == DCLAudioClip.LoadState.LOADING_COMPLETED)
                    {
                        ApplyLoadedAudioClip(dclAudioClip);
                    }
                    else
                    {
                        dclAudioClip.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
                        dclAudioClip.OnLoadingFinished += DclAudioClip_OnLoadingFinished;
                    }
                }
                else
                {
                    Debug.LogError("Wrong audio clip type when playing audiosource!!");
                }
            }
            else
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }

        private void OnAudioSettingsChanged(AudioSettings settings)
        {
            UpdateAudioSourceVolume();
        }

        private void OnVirtualAudioMixerChangedValue(float currentValue, float previousValue)
        {
            UpdateAudioSourceVolume();
        }

        private void UpdateAudioSourceVolume()
        {
            float newVolume = 0;

            // isOutOfBoundaries will always be false for global scenes.
            if (!isOutOfBoundaries)
            {
                AudioSettings audioSettingsData =
                    Settings.i != null ? Settings.i.audioSettings.Data : new AudioSettings();
                newVolume = ((Model) model).volume * Utils.ToVolumeCurve(
                    DataStore.i.virtualAudioMixer.sceneSFXVolume.Get() * audioSettingsData.sceneSFXVolume *
                    audioSettingsData.masterVolume);
            }

            bool isCurrentScene = scene.isPersistent || scene.sceneData.sceneNumber == CommonScriptableObjects.sceneNumber.Get();

            audioSource.volume = isCurrentScene ? newVolume : 0f;
        }

        private void OnCurrentSceneChanged(int currentSceneNumber, int previousSceneNumber)
        {
            if (audioSource == null)
                return;

            Model model = (Model) this.model;
            float volume = 0;

            if (scene.isPersistent || scene.sceneData.sceneNumber == currentSceneNumber)
            {
                volume = model.volume;
            }

            audioSource.volume = volume;
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            CommonScriptableObjects.sceneNumber.OnChange -= OnCurrentSceneChanged;

            //NOTE(Brian): Unsubscribe events.
            InitDCLAudioClip(null);

            if (Settings.i != null)
                Settings.i.audioSettings.OnChanged -= OnAudioSettingsChanged;

            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange -= OnVirtualAudioMixerChangedValue;
            DataStore.i.sceneBoundariesChecker.Remove(entity,this);

            lastDCLAudioClip = null;
            audioSource = null;
        }

        public void UpdateOutOfBoundariesState(bool isInsideBoundaries)
        {
            if (scene.isPersistent)
                isInsideBoundaries = true;

            isOutOfBoundaries = !isInsideBoundaries;
            UpdateAudioSourceVolume();
        }

        private void DclAudioClip_OnLoadingFinished(DCLAudioClip obj)
        {
            if (obj.loadingState == DCLAudioClip.LoadState.LOADING_COMPLETED && audioSource != null)
            {
                ApplyLoadedAudioClip(obj);
            }

            obj.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
        }

        private void ApplyLoadedAudioClip(DCLAudioClip clip)
        {
            if (audioSource.clip != clip.audioClip)
            {
                audioSource.clip = clip.audioClip;
            }

            Model model = (Model) this.model;
            bool shouldPlay = playedAtTimestamp != model.playedAtTimestamp ||
                              (model.playing && !audioSource.isPlaying);

            if (audioSource.enabled && model.playing && shouldPlay)
            {
                //To remove a pesky and quite unlikely warning when the audiosource is out of scenebounds
                audioSource.Play();
            }

            playedAtTimestamp = model.playedAtTimestamp;
        }

        public override int GetClassId() { return (int) CLASS_ID_COMPONENT.AUDIO_SOURCE; }
    }
}
