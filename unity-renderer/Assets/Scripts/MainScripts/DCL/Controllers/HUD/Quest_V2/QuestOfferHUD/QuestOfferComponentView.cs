using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Quest
{
    public class QuestOfferComponentView : BaseComponentView<QuestOfferComponentModel>, IQuestOfferComponentView
    {

        [SerializeField] internal TMP_Text questTitle;
        [SerializeField] internal TMP_Text questDescription;
        [SerializeField] internal Button acceptButton;
        [SerializeField] internal Button cancelButton;

        public event Action<string> OnQuestAccepted;

        public override void Awake()
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(()=>OnQuestAccepted?.Invoke(model.questId));
        }

        public override void RefreshControl()
        {
            SetQuestId(model.questId);
            SetQuestTitle(model.title);
            SetQuestDescription(model.description);
        }

        public void SetQuestId(string questId) =>
            model.questId = questId;

        public void SetQuestTitle(string title)
        {
            model.title = title;
            questTitle.text = title;
        }

        public void SetQuestDescription(string description)
        {
            model.description = description;
            questDescription.text = description;
        }
    }
}
