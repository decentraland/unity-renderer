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
        private const float DELAY_TO_DESTROY = 0.5f;
        private static readonly int OUT_ANIM_TRIGGER = Animator.StringToHash("Out");
        public event Action OnLayoutRebuildRequested;

        [SerializeField] internal TextMeshProUGUI questTitle;
        [SerializeField] internal RawImage questIcon;
        [SerializeField] internal TextMeshProUGUI sectionTitle;
        [SerializeField] internal Image progress;
        [SerializeField] internal RectTransform tasksContainer;
        [SerializeField] internal GameObject taskPrefab;
        [SerializeField] internal Button expandCollapseButton;
        [SerializeField] internal GameObject expandIcon;
        [SerializeField] internal GameObject collapseIcon;
        [SerializeField] internal Toggle pinQuestToggle;
        [SerializeField] internal RawImage iconImage;
        [SerializeField] internal Animator containerAnimator;

        private AssetPromise_Texture iconPromise;
        private float progressTarget = 0;

        internal QuestModel quest;
        private bool isExpanded;
        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;

        public void Awake()
        {
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);

            expandCollapseButton.gameObject.SetActive(false);
            SetExpandCollapseState(true);
            expandCollapseButton.onClick.AddListener(() => SetExpandCollapseState(!isExpanded));
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;
            questTitle.text = quest.name;
            SetIcon(quest.icon);
            QuestSection currentSection = quest.sections.First(x => x.progress < 1f);
            string separator = String.IsNullOrEmpty(currentSection.name) ? "" : " - ";
            sectionTitle.text = $"{currentSection.name}{separator}{(currentSection.progress * 100):0.0}%";
            progressTarget = currentSection.progress;

            CleanUpTasksList();
            foreach (QuestTask task in currentSection.tasks)
            {
                CreateTask(task);
            }
            expandCollapseButton.gameObject.SetActive(currentSection.tasks.Length > 0);
            SetExpandCollapseState(true);
        }

        internal void CreateTask(QuestTask task)
        {
            var taskUIEntry = Instantiate(taskPrefab, tasksContainer).GetComponent<QuestsTrackerTask>();
            taskUIEntry.Populate(task);
        }

        internal void SetIcon(string iconURL)
        {
            if (iconPromise != null)
            {
                iconPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(iconPromise);
            }

            if (string.IsNullOrEmpty(iconURL))
                return;

            iconPromise = new AssetPromise_Texture(iconURL);
            iconPromise.OnSuccessEvent += OnIconReady;
            iconPromise.OnFailEvent += x => { Debug.Log($"Error downloading quest tracker entry icon: {iconURL}"); };

            AssetPromiseKeeper_Texture.i.Keep(iconPromise);
        }

        private void OnIconReady(Asset_Texture assetTexture) { iconImage.texture = assetTexture.texture; }

        internal void CleanUpTasksList()
        {
            for (int i = tasksContainer.childCount - 1; i >= 0; i--)
                Destroy(tasksContainer.GetChild(i).gameObject);
        }

        internal void SetExpandCollapseState(bool newIsExpanded)
        {
            isExpanded = newIsExpanded;
            expandIcon.SetActive(!isExpanded);
            collapseIcon.SetActive(isExpanded);
            tasksContainer.gameObject.SetActive(isExpanded);

            foreach (QuestsTrackerTask task in GetComponentsInChildren<QuestsTrackerTask>())
            {
                task.SetExpandedStatus(newIsExpanded);
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

        public void SetPinStatus(bool isPinned) { pinQuestToggle.SetIsOnWithoutNotify(isPinned); }

        public void StartDestroy() { StartCoroutine(DestroyRoutine()); }

        private IEnumerator DestroyRoutine()
        {
            containerAnimator.SetTrigger(OUT_ANIM_TRIGGER);
            yield return WaitForSecondsCache.Get(DELAY_TO_DESTROY);

            OnLayoutRebuildRequested?.Invoke();
            Destroy(gameObject);
        }

        private void Update() { progress.fillAmount = Mathf.MoveTowards(progress.fillAmount, progressTarget, 0.1f); }

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