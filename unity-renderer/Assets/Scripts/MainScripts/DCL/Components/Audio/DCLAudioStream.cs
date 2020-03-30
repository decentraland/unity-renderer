using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class DCLAudioStream : BaseComponent
    {
        [System.Serializable]
        public class Model
        {
            public string url;
            public bool playing = false;
            public float volume = 1;
        }

        public Model model;
        private bool isPlaying = false;

        public override IEnumerator ApplyChanges(string newJson)
        {
            Model prevModel = model;
            model = SceneController.i.SafeFromJson<Model>(newJson);

            bool forceUpdate = prevModel.volume != model.volume;

            UpdatePlayingState(forceUpdate);

            yield return null;
        }

        private void Start()
        {
            CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChanged;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
        }

        private void OnDestroy()
        {
            CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChanged;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            StopStreaming();
        }

        private bool AreCoordsInsideComponentScene(Vector2Int coords)
        {
            if (scene == null) return false;
            return scene.parcels.Contains(coords);
        }

        private void UpdatePlayingState(bool forceStateUpdate)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            bool canPlayStream = AreCoordsInsideComponentScene(CommonScriptableObjects.playerCoords.Get()) && CommonScriptableObjects.rendererState.Get();

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

        private void OnPlayerCoordsChanged(Vector2Int coords, Vector2Int prevCoords)
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

        private void StopStreaming()
        {
            isPlaying = false;
            Interface.WebInterface.SendAudioStreamEvent(model.url, false, model.volume * AudioListener.volume);
        }

        private void StartStreaming()
        {
            isPlaying = true;
            Interface.WebInterface.SendAudioStreamEvent(model.url, true, model.volume * AudioListener.volume);
        }
    }
}
