using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerEntry : MonoBehaviour
    {
        internal const float OUT_ANIM_DELAY = 0.5f;
        private const float DELAY_TO_DESTROY = 0.5f;
        private static readonly int OUT_ANIM_TRIGGER = Animator.StringToHash("Out");
        public event Action OnLayoutRebuildRequested;
        public event Action<QuestModel> OnQuestCompleted;
        public event Action<QuestReward> OnRewardObtained;

        [SerializeField] internal TextMeshProUGUI questTitle;
        [SerializeField] internal TextMeshProUGUI questProgressText;
        [SerializeField] internal Image progress;
        [SerializeField] internal RectTransform sectionContainer;
        [SerializeField] internal GameObject sectionPrefab;
        [SerializeField] internal Button expandCollapseButton;
        [SerializeField] internal GameObject expandIcon;
        [SerializeField] internal GameObject collapseIcon;
        [SerializeField] internal Toggle pinQuestToggle;
        [SerializeField] internal RawImage iconImage;
        [SerializeField] internal Animator containerAnimator;

        public bool isReadyForDisposal { get; private set; } = false;
        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;
        private bool isProgressAnimationDone => Math.Abs(progress.fillAmount - progressTarget) < Mathf.Epsilon;

        private float progressTarget = 0;
        private AssetPromise_Texture iconPromise;

        internal QuestModel quest;
        internal bool isExpanded;
        internal bool isPinned;
        internal bool outAnimDone = false;
        internal bool hasProgressedThisUpdate = false;

        internal readonly Dictionary<string, QuestsTrackerSection> sectionEntries = new Dictionary<string, QuestsTrackerSection>();
        private readonly List<QuestReward> rewardsToNotify = new List<QuestReward>();
        private readonly List<Coroutine> sectionRoutines = new List<Coroutine>();
        private Coroutine sequenceRoutine;
        private Coroutine progressRoutine;

        public void Awake()
        {
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);

            expandCollapseButton.gameObject.SetActive(false);
            SetExpandCollapseState(true);
            expandCollapseButton.onClick.AddListener(() => SetExpandCollapseState(!isExpanded));
            StartCoroutine(OutDelayRoutine());

            AudioScriptableObjects.fadeIn.Play();
        }

        private IEnumerator OutDelayRoutine()
        {
            yield return new WaitForSeconds(OUT_ANIM_DELAY);
            outAnimDone = true;
        }

        public void Populate(QuestModel newQuest)
        {
            StopSequence();

            quest = newQuest;
            SetIcon(quest.thumbnail_entry);
            QuestTask[] allTasks = quest.sections.SelectMany(x => x.tasks).ToArray();

            int completedTasksAmount = allTasks.Count(x => x.progress >= 1);
            questTitle.text = $"{quest.name}";
            questProgressText.text = $"{completedTasksAmount}/{allTasks.Length}";
            progress.fillAmount = quest.oldProgress;
            progressTarget = quest.progress;

            hasProgressedThisUpdate = newQuest.sections.Any(x => x.tasks.Any(y => y.justProgressed));

            List<string> entriesToRemove = sectionEntries.Keys.ToList();
            List<QuestsTrackerSection> visibleSectionEntries = new List<QuestsTrackerSection>();
            List<QuestsTrackerSection> newSectionEntries = new List<QuestsTrackerSection>();
            for (var i = 0; i < quest.sections.Length; i++)
            {
                QuestSection section = quest.sections[i];

                bool hasTasks = section.tasks.Any(x => x.status != QuestsLiterals.Status.BLOCKED && (x.progress < 1 || x.justProgressed));
                if (!hasTasks)
                    continue;

                bool isVisible = section.tasks.Any(x => x.status != QuestsLiterals.Status.BLOCKED && ((x.progress < 1 && !x.justUnlocked) || (x.progress >= 1 && x.justProgressed)));

                entriesToRemove.Remove(section.id);
                if (!sectionEntries.TryGetValue(section.id, out QuestsTrackerSection sectionEntry))
                {
                    sectionEntry = CreateSection();
                    //New tasks are invisible
                    sectionEntries.Add(section.id, sectionEntry);
                }

                sectionEntry.gameObject.SetActive(isVisible);
                sectionEntry.Populate(section);
                sectionEntry.transform.SetAsLastSibling();

                if (sectionEntry.gameObject.activeSelf)
                    visibleSectionEntries.Add(sectionEntry);
                else
                    newSectionEntries.Add(sectionEntry);
            }

            for (int index = 0; index < entriesToRemove.Count; index++)
            {
                DestroySectionEntry(entriesToRemove[index]);
            }

            expandCollapseButton.gameObject.SetActive(sectionEntries.Count > 0);
            SetExpandCollapseState(true);
            OnLayoutRebuildRequested?.Invoke();

            sequenceRoutine = StartCoroutine(Sequence(visibleSectionEntries, newSectionEntries));
        }

        private QuestsTrackerSection CreateSection()
        {
            var sectionEntry = Instantiate(sectionPrefab, sectionContainer).GetComponent<QuestsTrackerSection>();
            sectionEntry.OnLayoutRebuildRequested += () => OnLayoutRebuildRequested?.Invoke();
            sectionEntry.OnDestroyed += (sectionId) => sectionEntries.Remove(sectionId);
            return sectionEntry;
        }

        private void DestroySectionEntry(string taskId)
        {
            if (!sectionEntries.TryGetValue(taskId, out QuestsTrackerSection sectionEntry))
                return;
            Destroy(sectionEntry.gameObject);
            sectionEntries.Remove(taskId);
        }

        internal void SetIcon(string iconURL)
        {
            if (iconPromise != null)
            {
                iconPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(iconPromise);
            }

            if (string.IsNullOrEmpty(iconURL))
            {
                iconImage.gameObject.SetActive(false);
                return;
            }

            iconPromise = new AssetPromise_Texture(iconURL);
            iconPromise.OnSuccessEvent += assetTexture =>
            {
                iconImage.gameObject.SetActive(true);
                iconImage.texture = assetTexture.texture;
            };
            iconPromise.OnFailEvent += assetTexture =>
            {
                iconImage.gameObject.SetActive(false);
                Debug.Log($"Error downloading quest tracker entry icon: {iconURL}");
            };

            AssetPromiseKeeper_Texture.i.Keep(iconPromise);
        }

        private IEnumerator Sequence(List<QuestsTrackerSection> visibleSections, List<QuestsTrackerSection> newSections)
        {
            yield return new WaitUntil(() => outAnimDone);

            ClearSectionRoutines();

            if (progressRoutine != null)
                StopCoroutine(progressRoutine);
            progressRoutine = StartCoroutine(ProgressSequence());

            //Progress of currently visible sections
            for (int i = 0; i < visibleSections.Count; i++)
            {
                sectionRoutines.Add(StartCoroutine(visibleSections[i].Sequence()));
            }

            yield return WaitForTaskRoutines();

            //Show and progress of new tasks
            for (int i = 0; i < newSections.Count; i++)
            {
                newSections[i].gameObject.SetActive(true);
                sectionRoutines.Add(StartCoroutine(newSections[i].Sequence()));
            }
            OnLayoutRebuildRequested?.Invoke();
            yield return WaitForTaskRoutines();

            OnLayoutRebuildRequested?.Invoke();
            //The entry should exit automatically if questCompleted or no progress, therefore the use of MinValue
            DateTime tasksIdleTime = (quest.isCompleted || !hasProgressedThisUpdate) ? DateTime.MinValue : DateTime.Now;
            yield return new WaitUntil(() => isProgressAnimationDone && !isPinned && (DateTime.Now - tasksIdleTime) > TimeSpan.FromSeconds(3));

            if (quest.isCompleted)
                OnQuestCompleted?.Invoke(quest);

            for (int i = 0; i < rewardsToNotify.Count; i++)
            {
                OnRewardObtained?.Invoke(rewardsToNotify[i]);
            }
            rewardsToNotify.Clear();

            isReadyForDisposal = true;
        }

        private void StopSequence()
        {
            if (sequenceRoutine != null)
                StopCoroutine(sequenceRoutine);
            ClearSectionRoutines();
        }

        private void ClearSectionRoutines()
        {
            if (sectionRoutines.Count > 0)
            {
                for (int i = 0; i < sectionRoutines.Count; i++)
                {
                    if (sectionRoutines[i] != null)
                        StopCoroutine(sectionRoutines[i]);
                }
                sectionRoutines.Clear();
            }
            foreach (var section in sectionEntries)
            {
                section.Value.ClearTaskRoutines();
            }
        }

        internal void SetExpandCollapseState(bool newIsExpanded)
        {
            isExpanded = newIsExpanded;
            expandIcon.SetActive(!isExpanded);
            collapseIcon.SetActive(isExpanded);
            sectionContainer.gameObject.SetActive(isExpanded);

            foreach (QuestsTrackerSection section in sectionEntries.Values)
            {
                section.SetExpandCollapseState(newIsExpanded);
            }

            OnLayoutRebuildRequested?.Invoke();
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (!quest.canBePinned)
            {
                pinnedQuests.Remove(quest.id);
                SetPinStatus(false);
                return;
            }

            if (isOn)
            {
                if (!pinnedQuests.Contains(quest.id))
                    pinnedQuests.Add(quest.id);
            }
            else
            {
                pinnedQuests.Remove(quest.id);
            }
        }

        public void SetPinStatus(bool newIsPinned)
        {
            isPinned = newIsPinned;
            pinQuestToggle.SetIsOnWithoutNotify(newIsPinned);
        }

        public void AddRewardToGive(QuestReward reward) { rewardsToNotify.Add(reward); }

        public void StartDestroy() { StartCoroutine(DestroySequence()); }

        private IEnumerator DestroySequence()
        {
            AudioScriptableObjects.fadeOut.Play();
            containerAnimator.SetTrigger(OUT_ANIM_TRIGGER);
            yield return WaitForSecondsCache.Get(DELAY_TO_DESTROY);

            OnLayoutRebuildRequested?.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator ProgressSequence()
        {
            while (Math.Abs(progress.fillAmount - progressTarget) > Mathf.Epsilon)
            {
                progress.fillAmount = Mathf.MoveTowards(progress.fillAmount, progressTarget, Time.deltaTime);
                yield return null;
            }
            progressRoutine = null;
        }

        private IEnumerator WaitForTaskRoutines()
        {
            for (int i = 0; i < sectionRoutines.Count; i++)
            {
                //yielding Coroutines (not IEnumerators) allows us to wait for them in parallel
                yield return sectionRoutines[i];
            }
        }

        private void OnDestroy()
        {
            if (iconPromise != null)
            {
                iconPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(iconPromise);
            }
        }
    }

}