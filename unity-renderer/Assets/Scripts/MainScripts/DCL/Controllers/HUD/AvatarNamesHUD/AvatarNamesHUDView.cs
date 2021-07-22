using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AvatarNamesHUD
{
    public interface IAvatarNamesHUDView
    {
        void SetVisibility(bool visibility);
        void TrackPlayer(PlayerStatus playerStatus);
        void UntrackPlayer(PlayerStatus playerStatus);
    }

    public class AvatarNamesHUDView : MonoBehaviour, IAvatarNamesHUDView
    {
        private const float TRACKERS_UPDATE_DELAY = 0.2f;
        public static Vector3 offset = new Vector3(0, 2, 0);

        [SerializeField] internal RectTransform canvasRect;
        [SerializeField] internal RectTransform backgroundsContainer;
        [SerializeField] internal RectTransform namesContainer;
        [SerializeField] internal GameObject backgroundPrefab;
        [SerializeField] internal GameObject namePrefab;

        private readonly Dictionary<string, AvatarNamesTracker> trackers = new Dictionary<string, AvatarNamesTracker>();
        private readonly Queue<AvatarNamesTracker> reserveTrackers = new Queue<AvatarNamesTracker>();

        public Vector3 pos;
        private void LateUpdate() { offset = pos; }

        private void Awake()
        {
            //Prewarming trackers
            for (int i = 0; i < AvatarNamesHUDController.MAX_AVATAR_NAMES; i++)
            {
                RectTransform background = Instantiate(backgroundPrefab, backgroundsContainer).GetComponent<RectTransform>();
                TextMeshProUGUI nameTMP = Instantiate(namePrefab, namesContainer).GetComponent<TextMeshProUGUI>();
                AvatarNamesTracker tracker = new AvatarNamesTracker(canvasRect, background, nameTMP);
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
                //yield return WaitForSecondsCache.Get(TRACKERS_UPDATE_DELAY);
            }
        }
    }
}