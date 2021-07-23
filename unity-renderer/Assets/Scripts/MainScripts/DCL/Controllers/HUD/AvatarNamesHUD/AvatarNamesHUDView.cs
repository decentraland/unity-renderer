using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AvatarNamesHUD
{
    public interface IAvatarNamesHUDView
    {
        void SetVisibility(bool visibility);
        void TrackPlayer(PlayerStatus playerStatus);
        void UntrackPlayer(PlayerStatus playerStatus);
        void Dispose();
    }

    public class AvatarNamesHUDView : MonoBehaviour, IAvatarNamesHUDView
    {
        [SerializeField] internal RectTransform canvasRect;
        [SerializeField] internal RectTransform backgroundsContainer;
        [SerializeField] internal RectTransform namesContainer;
        [SerializeField] internal RectTransform voiceChatContainer;
        [SerializeField] internal GameObject backgroundPrefab;
        [SerializeField] internal GameObject namePrefab;
        [SerializeField] internal GameObject voiceChatPrefab;

        private readonly Dictionary<string, AvatarNamesTracker> trackers = new Dictionary<string, AvatarNamesTracker>();
        private readonly Queue<AvatarNamesTracker> reserveTrackers = new Queue<AvatarNamesTracker>();

        private bool isDestroyed = false;

        private void Awake()
        {
            //Prewarming trackers
            for (int i = 0; i < AvatarNamesHUDController.MAX_AVATAR_NAMES; i++)
            {
                RectTransform background = Instantiate(backgroundPrefab, backgroundsContainer).GetComponent<RectTransform>();
                RectTransform nameTMP = Instantiate(namePrefab, namesContainer).GetComponent<RectTransform>();
                RectTransform voiceChat = Instantiate(voiceChatPrefab, voiceChatContainer).GetComponent<RectTransform>();
                AvatarNamesTracker tracker = new AvatarNamesTracker(canvasRect, background, nameTMP, voiceChat);
                tracker.SetVisibility(false);
                reserveTrackers.Enqueue(tracker);
            }

            StartCoroutine(UpdateTrackersRoutine());
        }

        public static IAvatarNamesHUDView CreateView()
        {
            AvatarNamesHUDView view = Instantiate(Resources.Load<GameObject>("AvatarNamesHUD")).GetComponent<AvatarNamesHUDView>();
            view.gameObject.name = "_AvatarNamesHUD";
            return view;
        }

        public void SetVisibility(bool visibility) { gameObject.SetActive(visibility); }

        public void TrackPlayer(PlayerStatus playerStatus)
        {
            if (reserveTrackers.Count == 0)
            {
                Debug.LogError("No tracker available for this AvatarName");
                return;
            }

            AvatarNamesTracker tracker = reserveTrackers.Dequeue();
            tracker.SetPlayerStatus(playerStatus);
            tracker.SetVisibility(true);
            trackers.Add(playerStatus.id, tracker);
        }

        public void UntrackPlayer(PlayerStatus playerStatus)
        {
            if (!trackers.TryGetValue(playerStatus.id, out AvatarNamesTracker tracker))
                return;

            trackers.Remove(playerStatus.id);
            tracker.SetPlayerStatus(null);
            tracker.SetVisibility(false);
            reserveTrackers.Enqueue(tracker);
        }

        private IEnumerator UpdateTrackersRoutine()
        {
            while (true)
            {
                foreach (KeyValuePair<string, AvatarNamesTracker> kvp in trackers)
                {
                    kvp.Value.UpdatePosition();
                }
                yield return null;
            }
        }

        private void OnDestroy() { isDestroyed = true; }

        public void Dispose()
        {
            if (isDestroyed)
                return;
            isDestroyed = true;
            Destroy(gameObject);
        }
    }
}