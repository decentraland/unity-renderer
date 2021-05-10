using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelPopup : MonoBehaviour
    {
        private const int ARROW_OFFSET = 220;

        [SerializeField] internal TextMeshProUGUI questName;
        [SerializeField] internal TextMeshProUGUI description;
        [SerializeField] internal RectTransform sectionsContainer;
        [SerializeField] internal GameObject sectionPrefab;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal Toggle pinQuestToggle;
        [SerializeField] internal RawImage thumbnailImage;
        [SerializeField] internal Button closePopupAreaButton;
        [SerializeField] private DynamicScrollSensitivity dynamicScrollSensitivity;
        [SerializeField] internal GameObject rewardsPanel;
        [SerializeField] internal TextMeshProUGUI rewardsAmount;

        private AssetPromise_Texture thumbnailPromise;
        private bool forceRebuildLayout = false;

        internal QuestModel quest;
        internal readonly List<QuestsPanelSection> sections = new List<QuestsPanelSection>();
        private static BaseCollection<string> baseCollection => DataStore.i.Quests.pinnedQuests;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            closeButton.onClick.AddListener(Close);

            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);
            baseCollection.OnAdded += OnPinnedQuests;
            baseCollection.OnRemoved += OnUnpinnedQuest;

            closePopupAreaButton.onClick.AddListener(Close);
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;
            QuestSection[] availableSections = quest.sections.Where(x => x.tasks.Any(y => y.status != QuestsLiterals.Status.BLOCKED)).ToArray();
            PrepareSections(availableSections.Length);

            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_banner);
            for (int i = 0; i < availableSections.Length; i++)
            {
                sections[i].Populate(availableSections[i]);
            }
            pinQuestToggle.SetIsOnWithoutNotify(baseCollection.Contains(quest.id));
            pinQuestToggle.gameObject.SetActive(!quest.isCompleted);
            forceRebuildLayout = true;

            SetRewards(quest.rewards?.Length ?? 0);
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (isOn)
            {
                if (!baseCollection.Contains(quest.id))
                    baseCollection.Add(quest.id);
            }
            else
            {
                if (baseCollection.Contains(quest.id))
                    baseCollection.Remove(quest.id);
            }
        }

        private void OnPinnedQuests(string questId)
        {
            if (quest != null && quest.id == questId)
                pinQuestToggle.SetIsOnWithoutNotify(true);
        }

        private void OnUnpinnedQuest(string questId)
        {
            if (quest != null && quest.id == questId)
                pinQuestToggle.SetIsOnWithoutNotify(false);
        }

        internal void SetThumbnail(string thumbnailURL)
        {
            if (thumbnailPromise != null)
            {
                thumbnailPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            }

            if (string.IsNullOrEmpty(thumbnailURL))
            {
                thumbnailImage.gameObject.SetActive(false);
                return;
            }

            thumbnailPromise = new AssetPromise_Texture(thumbnailURL);
            thumbnailPromise.OnSuccessEvent += OnThumbnailReady;
            thumbnailPromise.OnFailEvent += x =>
            {
                thumbnailImage.gameObject.SetActive(false);
                Debug.Log($"Error downloading quest panel popup thumbnail: {thumbnailURL}");
            };

            AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
        }

        private void OnThumbnailReady(Asset_Texture assetTexture)
        {
            thumbnailImage.gameObject.SetActive(true);
            thumbnailImage.texture = assetTexture.texture;
        }

        internal void CreateSection() { sections.Add(Instantiate(sectionPrefab, sectionsContainer).GetComponent<QuestsPanelSection>()); }

        internal void PrepareSections(int sectionsAmount)
        {
            if (sections.Count == sectionsAmount)
                return;

            if (sections.Count < sectionsAmount)
            {
                while (sections.Count < sectionsAmount)
                    CreateSection();
            }
            else
            {
                while (sections.Count > sectionsAmount)
                {
                    var section = sections.Last();
                    sections.RemoveAt(sections.Count - 1);
                    Destroy(section.gameObject);
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            AudioScriptableObjects.dialogOpen.Play();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            AudioScriptableObjects.dialogClose.Play();
        }

        private void Update()
        {
            if (forceRebuildLayout)
            {
                forceRebuildLayout = false;
                rectTransform.ForceUpdateLayout();
                dynamicScrollSensitivity?.RecalculateSensitivity();
            }
        }

        private void SetRewards(int amount)
        {
            rewardsPanel.SetActive(amount > 0);
            rewardsAmount.text = amount.ToString();
        }

        private void OnDestroy()
        {
            if (thumbnailPromise != null)
            {
                thumbnailPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            }
        }

        public void SetPositionToReadMoreButton(Vector3 position)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Max(0, position.y - ARROW_OFFSET);
            transform.position = pos;
        }
    }
}