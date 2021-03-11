using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;

namespace DCL.Components
{
    public class DCLAudioStream : BaseComponent, IOutOfSceneBoundariesHandler
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public string url;
            public bool playing = false;
            public float volume = 1;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        private void Awake() { model = new Model(); }

        private bool isPlaying = false;
        private float settingsVolume = 0;
        private bool isDestroyed = false;
        private Model prevModel = new Model();

        new public Model GetModel() { return (Model) model;}
        
        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDestroyed)
                yield break;

            Model model = (Model)newModel;
            bool forceUpdate = prevModel.volume != model.volume;
            settingsVolume = Settings.i.generalSettings.sfxVolume;

            UpdatePlayingState(forceUpdate);
            prevModel = model;
            yield return null;
        }

        private void Start()
        {
            CommonScriptableObjects.sceneID.OnChange += OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
            Settings.i.OnGeneralSettingsChanged += OnSettingsChanged;
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            CommonScriptableObjects.sceneID.OnChange -= OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            Settings.i.OnGeneralSettingsChanged -= OnSettingsChanged;
            StopStreaming();
        }

        private bool IsPlayerInSameSceneAsComponent(string currentSceneId)
        {
            if (scene == null) return false;
            if (string.IsNullOrEmpty(currentSceneId)) return false;
            return (scene.sceneData.id == currentSceneId) || (scene is GlobalScene globalScene && globalScene.isPortableExperience);
        }

        private void UpdatePlayingState(bool forceStateUpdate)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            bool canPlayStream = IsPlayerInSameSceneAsComponent(CommonScriptableObjects.sceneID) && CommonScriptableObjects.rendererState;

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
                if (isPlaying) StartStreaming();
                else StopStreaming();
            }
        }

        private void OnSceneChanged(string sceneId, string prevSceneId)
        {
            UpdatePlayingState(false);
        }

        private void OnRendererStateChanged(bool isEnable, bool prevState)
        {
            if (isEnable)
            {
                UpdatePlayingState(false);
            }
        }

        private void OnSettingsChanged(SettingsData.GeneralSettings settings)
        {
            if (settingsVolume != settings.sfxVolume)
            {
                settingsVolume = settings.sfxVolume;
                UpdatePlayingState(true);
            }
        }

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

        public void UpdateOutOfBoundariesState(bool enable)
        {
            if (!isPlaying)
                return;

            if (enable)
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

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.AUDIO_STREAM;
        }
    }
}