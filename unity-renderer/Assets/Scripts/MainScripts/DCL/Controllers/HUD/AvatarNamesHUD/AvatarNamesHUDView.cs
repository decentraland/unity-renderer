using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using UnityEngine;

namespace AvatarNamesHUD
{
    public interface IAvatarNamesHUDView
    {
        void Initialize();
        void SetVisibility(bool visibility);
        void TrackPlayer(Player player);
        void UntrackPlayer(string userId);
        void Dispose();
    }

    public class AvatarNamesHUDView : MonoBehaviour, IAvatarNamesHUDView
    {
        internal const int MAX_AVATAR_NAMES = 100; //Regardless of what the dataStore says, we will show up to 100 names (we shouldnt hit this number)
        internal const int INITIAL_RESERVE_SIZE = 50; //Regardless of what the dataStore says, we will show up to 100 names (we shouldnt hit this number)

        [SerializeField] internal RectTransform canvasRect;
        [SerializeField] internal RectTransform backgroundsContainer;
        [SerializeField] internal RectTransform namesContainer;
        [SerializeField] internal RectTransform voiceChatContainer;
        [SerializeField] internal GameObject backgroundPrefab;
        [SerializeField] internal GameObject namePrefab;
        [SerializeField] internal GameObject voiceChatPrefab;

        internal readonly Dictionary<string, AvatarNamesTracker> trackers = new Dictionary<string, AvatarNamesTracker>();
        internal readonly Queue<AvatarNamesTracker> reserveTrackers = new Queue<AvatarNamesTracker>();
        internal BaseHashSet<string> visibleNames => DataStore.i.avatarsLOD.visibleNames;

        internal bool isDestroyed = false;

        public static IAvatarNamesHUDView CreateView()
        {
            AvatarNamesHUDView view = Instantiate(Resources.Load<GameObject>("AvatarNamesHUD")).GetComponent<AvatarNamesHUDView>();
            view.gameObject.name = "_AvatarNamesHUD";
            return view;
        }

        private void OnEnable()
        {
            canvasRect.GetComponent<Canvas>().sortingOrder = -2;
            StartCoroutine(UpdateTrackersRoutine());
        }

        public void Initialize()
        {
            //Return current trackers to reserve
            trackers.Keys.ToList().ForEach(UntrackPlayer);
            trackers.Clear();

            //Fill the reserve
            while (reserveTrackers.Count < INITIAL_RESERVE_SIZE)
            {
                AvatarNamesTracker tracker = CreateTracker();
                reserveTrackers.Enqueue(tracker);
            }
        }

        private AvatarNamesTracker CreateTracker()
        {
            RectTransform background = Instantiate(backgroundPrefab, backgroundsContainer).GetComponent<RectTransform>();
            RectTransform nameTMP = Instantiate(namePrefab, namesContainer).GetComponent<RectTransform>();
            RectTransform voiceChat = Instantiate(voiceChatPrefab, voiceChatContainer).GetComponent<RectTransform>();
            AvatarNamesTracker tracker = new AvatarNamesTracker(canvasRect, background, nameTMP, voiceChat);
            tracker.SetVisibility(false);
            return tracker;
        }

        public void SetVisibility(bool visibility) { gameObject.SetActive(visibility); }

        public void TrackPlayer(Player player)
        {
            if (reserveTrackers.Count == 0)
                reserveTrackers.Enqueue(CreateTracker());

            AvatarNamesTracker tracker = reserveTrackers.Dequeue();
            tracker.SetPlayer(player);
            tracker.SetVisibility(true);
            trackers.Add(player.id, tracker);
        }

        public void UntrackPlayer(string userId)
        {
            if (!trackers.TryGetValue(userId, out AvatarNamesTracker tracker))
                return;

            trackers.Remove(userId);
            tracker.SetPlayer(null);
            tracker.SetVisibility(false);
            reserveTrackers.Enqueue(tracker);
        }

        private IEnumerator UpdateTrackersRoutine()
        {
            while (true)
            {
                int count = 0;
                foreach (KeyValuePair<string, AvatarNamesTracker> kvp in trackers)
                {
                    if (count >= MAX_AVATAR_NAMES || !visibleNames.Contains(kvp.Key))
                    {
                        kvp.Value.SetVisibility(false);
                        continue;
                    }
                    kvp.Value.SetVisibility(true);
                    kvp.Value.UpdatePosition();
                    count++;
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