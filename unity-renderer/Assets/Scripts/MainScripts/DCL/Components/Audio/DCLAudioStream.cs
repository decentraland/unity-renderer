using System;
using System.Collections;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using DCL.SettingsCommon;
using UnityEngine;
using AudioSettings = DCL.SettingsCommon.AudioSettings;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class DCLAudioStream : BaseComponent, IOutOfSceneBoundariesHandler
    {
        [Serializable]
        public class Model : BaseModel
        {
            public string url;
            public bool playing;
            public float volume = 1;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.AudioStream)
                    return Utils.SafeUnimplemented<DCLAudioStream, Model>(expected: ComponentBodyPayload.PayloadOneofCase.AudioStream, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.AudioStream.HasPlaying) pb.playing = pbModel.AudioStream.Playing;
                if (pbModel.AudioStream.HasUrl) pb.url = pbModel.AudioStream.Url;
                if (pbModel.AudioStream.HasVolume) pb.volume = pbModel.AudioStream.Volume;

                return pb;
            }
        }

        private void Awake() { model = new Model(); }

        public override void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            base.Initialize(scene, entity);
            DataStore.i.sceneBoundariesChecker.Add(entity,this);
        }

        public bool isPlaying { get; private set; } = false;
        private float settingsVolume = 0;
        private bool isDestroyed = false;
        private Model prevModel = new Model();

        public override string componentName => "AudioStream";

        new public Model GetModel() { return (Model) model; }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDestroyed)
                yield break;

            Model model = (Model)newModel;
            bool forceUpdate = prevModel.volume != model.volume;
            settingsVolume = GetCalculatedSettingsVolume(Settings.i.audioSettings.Data);

            UpdatePlayingState(forceUpdate);
            prevModel = model;
            yield return null;
        }

        private void Start()
        {
            CommonScriptableObjects.sceneNumber.OnChange += OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
            Settings.i.audioSettings.OnChanged += OnSettingsChanged;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange += SceneSFXVolume_OnChange;
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            CommonScriptableObjects.sceneNumber.OnChange -= OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            Settings.i.audioSettings.OnChanged -= OnSettingsChanged;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange -= SceneSFXVolume_OnChange;
            StopStreaming();
            DataStore.i.sceneBoundariesChecker.Remove(entity,this);
        }

        private void UpdatePlayingState(bool forceStateUpdate)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            bool canPlayStream = scene.isPersistent || scene.sceneData.sceneNumber == CommonScriptableObjects.sceneNumber.Get();
            canPlayStream &= CommonScriptableObjects.rendererState;

            Model model = (Model) this.model;
            bool shouldStopStream = (isPlaying && !model.playing) || (isPlaying && !canPlayStream);
            bool shouldStartStream = !isPlaying && canPlayStream && model.playing;

            if (shouldStopStream)
            {
                StopStreaming();
                return;
            }

            if (shouldStartStream)
            {
                StartStreaming();
                return;
            }

            if (forceStateUpdate)
            {
                if (isPlaying)
                    StartStreaming();
                else
                    StopStreaming();
            }
        }

        private void OnSceneChanged(int currentSceneNumber, int previousSceneNumber) { UpdatePlayingState(false); }

        private void OnRendererStateChanged(bool isEnable, bool prevState)
        {
            if (isEnable)
            {
                UpdatePlayingState(false);
            }
        }

        private void OnSettingsChanged(AudioSettings settings)
        {
            float newSettingsVolume = GetCalculatedSettingsVolume(settings);
            if (Math.Abs(settingsVolume - newSettingsVolume) > Mathf.Epsilon)
            {
                settingsVolume = newSettingsVolume;
                UpdatePlayingState(true);
            }
        }

        private float GetCalculatedSettingsVolume(AudioSettings audioSettings) { return Utils.ToVolumeCurve(DataStore.i.virtualAudioMixer.sceneSFXVolume.Get() * audioSettings.sceneSFXVolume * audioSettings.masterVolume); }

        private void SceneSFXVolume_OnChange(float current, float previous) { OnSettingsChanged(Settings.i.audioSettings.Data); }

        private void StopStreaming()
        {
            Model model = (Model) this.model;
            isPlaying = false;
            Interface.WebInterface.SendAudioStreamEvent(model.url, false, model.volume * settingsVolume);
        }

        private void StartStreaming()
        {
            Model model = (Model) this.model;
            isPlaying = true;
            Interface.WebInterface.SendAudioStreamEvent(model.url, true, model.volume * settingsVolume);
        }

        public void UpdateOutOfBoundariesState(bool isInsideBoundaries)
        {
            if (!isPlaying)
                return;

            if (isInsideBoundaries)
            {
                StartStreaming();
            }
            else
            {
                Model model = (Model) this.model;
                //Set volume to 0 (temporary solution until the refactor in #1421)
                Interface.WebInterface.SendAudioStreamEvent(model.url, true, 0);
            }
        }

        public override int GetClassId() { return (int) CLASS_ID_COMPONENT.AUDIO_STREAM; }
    }
}
