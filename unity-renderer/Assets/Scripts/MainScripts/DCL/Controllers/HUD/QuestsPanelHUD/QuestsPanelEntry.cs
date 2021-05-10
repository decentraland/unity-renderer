using DCL.Helpers;
using DCL.Interface;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelEntry : MonoBehaviour
    {
        private static readonly int LOADED_ANIM_TRIGGER = Animator.StringToHash("Loaded");
        public event Action<string> OnReadMoreClicked;

        [SerializeField] internal TextMeshProUGUI questName;
        [SerializeField] internal TextMeshProUGUI description;
        [SerializeField] internal Button readMoreButton;
        [SerializeField] internal Toggle pinQuestToggle;
        [SerializeField] internal Image progressInTitle;
        [SerializeField] internal RectTransform completedProgressInTitle;
        [SerializeField] internal RectTransform completedMarkInTitle;
        [SerializeField] internal RawImage thumbnailImage;
        [SerializeField] internal Button jumpInButton;
        [SerializeField] internal Animator animator;
        [SerializeField] internal GameObject rewardsPanel;
        [SerializeField] internal TextMeshProUGUI rewardsAmount;

        private AssetPromise_Texture thumbnailPromise;

        private QuestModel quest;
        private string currentThumbnail;

        internal Action readMoreDelegate;
        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;

        private Action jumpInDelegate;
        public Vector3 readMorePosition => readMoreButton.transform.position;

        private bool isDestroyed = false;

        private void Awake()
        {
            jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke(); });
            readMoreButton.onClick.AddListener(() => readMoreDelegate?.Invoke());
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);
            pinnedQuests.OnAdded += OnPinnedQuests;
            pinnedQuests.OnRemoved += OnUnpinnedQuest;
        }

        private void OnEnable()
        {
            if (quest == null || string.IsNullOrEmpty(currentThumbnail) || (thumbnailPromise != null && thumbnailPromise.state != AssetPromiseState.LOADING))
                animator?.SetTrigger(LOADED_ANIM_TRIGGER);
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;

            QuestTask incompletedTask = quest.sections.FirstOrDefault(x => x.progress < 1)?.tasks.FirstOrDefault(x => x.progress < 1);
            jumpInButton.gameObject.SetActive(incompletedTask != null && !string.IsNullOrEmpty(incompletedTask?.coordinates));
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {incompletedTask?.coordinates}",
            });

            readMoreDelegate = () => OnReadMoreClicked?.Invoke(quest.id);
            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_entry);
            pinQuestToggle.SetIsOnWithoutNotify(pinnedQuests.Contains(quest.id));

            pinQuestToggle.gameObject.SetActive(!quest.isCompleted);
            progressInTitle.transform.localScale = new Vector3(quest.progress, 1, 1);
            completedProgressInTitle.gameObject.SetActive(quest.isCompleted);
            completedMarkInTitle.gameObject.SetActive(quest.isCompleted);

            SetRewards(quest.rewards?.Length ?? 0);
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (!quest.canBePinned)
            {
                pinnedQuests.Remove(quest.id);
                pinQuestToggle.SetIsOnWithoutNotify(false);
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
            if (thumbnailURL == currentThumbnail)
            {
                animator.SetTrigger(LOADED_ANIM_TRIGGER);
                return;
            }

            currentThumbnail = thumbnailURL;
            if (thumbnailPromise != null)
            {
                thumbnailPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            }

            if (string.IsNullOrEmpty(currentThumbnail))
            {
                thumbnailImage.gameObject.SetActive(false);
                animator.SetTrigger(LOADED_ANIM_TRIGGER);
                return;
            }

            thumbnailPromise = new AssetPromise_Texture(currentThumbnail);
            thumbnailPromise.OnSuccessEvent += OnThumbnailReady;
            thumbnailPromise.OnFailEvent += x =>
            {
                thumbnailImage.gameObject.SetActive(false);
                animator.SetTrigger(LOADED_ANIM_TRIGGER);
                Debug.LogError($"Error downloading quest panel entry thumbnail: {currentThumbnail}");
            };

            AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
        }

        private void OnThumbnailReady(Asset_Texture assetTexture)
        {
            thumbnailImage.gameObject.SetActive(true);
            thumbnailImage.texture = assetTexture.texture;
            animator.SetTrigger(LOADED_ANIM_TRIGGER);
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
            pinnedQuests.OnAdded -= OnUnpinnedQuest;
            pinnedQuests.OnRemoved -= OnPinnedQuests;
            isDestroyed = true;
        }

        public void Unparent()
        {
            if (isDestroyed)
                return;
            transform.parent = null;
        }

        public void SelfDestroy()
        {
            if (isDestroyed)
                return;
            Destroy(gameObject);
        }
    }
}