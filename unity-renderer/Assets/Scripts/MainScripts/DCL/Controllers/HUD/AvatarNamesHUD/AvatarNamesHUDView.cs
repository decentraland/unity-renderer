using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AvatarNamesHUD
{
    public interface IAvatarNamesHUDView
    {
        void Initialize(int maxAvatarNames);
        void SetVisibility(bool visibility);
        void TrackPlayer(PlayerStatus playerStatus);
        void UntrackPlayer(string userId);
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

        internal readonly Dictionary<string, AvatarNamesTracker> trackers = new Dictionary<string, AvatarNamesTracker>();
        internal readonly Queue<AvatarNamesTracker> reserveTrackers = new Queue<AvatarNamesTracker>();

        private bool isDestroyed = false;

        public static IAvatarNamesHUDView CreateView()
        {
            AvatarNamesHUDView view = Instantiate(Resources.Load<GameObject>("AvatarNamesHUD")).GetComponent<AvatarNamesHUDView>();
            view.gameObject.name = "_AvatarNamesHUD";
            return view;
        }

        private void OnEnable() { StartCoroutine(UpdateTrackersRoutine()); }

        public void Initialize(int maxAvatarNames)
        {
            //Return current trackers to reserve
            trackers.Keys.ToList().ForEach(UntrackPlayer);
            trackers.Clear();

            //Remove exceeding trackers in the reserve
            while (reserveTrackers.Count > maxAvatarNames)
            {
                AvatarNamesTracker tracker = reserveTrackers.Dequeue();
                tracker.DestroyUIElements();
            }

            //Fill the reserve if not ready
            for (int i = reserveTrackers.Count; i < maxAvatarNames; i++)
            {
                RectTransform background = Instantiate(backgroundPrefab, backgroundsContainer).GetComponent<RectTransform>();
                RectTransform nameTMP = Instantiate(namePrefab, namesContainer).GetComponent<RectTransform>();
                RectTransform voiceChat = Instantiate(voiceChatPrefab, voiceChatContainer).GetComponent<RectTransform>();
                AvatarNamesTracker tracker = new AvatarNamesTracker(canvasRect, background, nameTMP, voiceChat);
                tracker.SetVisibility(false);
                reserveTrackers.Enqueue(tracker);
            }
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

        public void UntrackPlayer(string userId)
        {
            if (!trackers.TryGetValue(userId, out AvatarNamesTracker tracker))
                return;

            trackers.Remove(userId);
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