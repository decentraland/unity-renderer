using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace DCL.Quests
{
    public class QuestStartedPopupComponentView : BaseComponentView<QuestStartedPopupComponentModel>, IQuestStartedPopupComponentView
    {
        private const float DURATION = 0.167f;

        [SerializeField] internal TMP_Text questNameText;
        [SerializeField] internal Button openQuestLogButton;
        [SerializeField] internal CanvasGroup container;

        public event Action OnOpenQuestLog;
        internal bool visible = false;

        public override void Awake()
        {
            openQuestLogButton.onClick.RemoveAllListeners();
            openQuestLogButton.onClick.AddListener(()=>OnOpenQuestLog?.Invoke());
            container.alpha = 0;
        }

        public override void RefreshControl() =>
            SetQuestName(model.questName);

        public void SetQuestName(string questName)
        {
            model.questName = questName;
            questNameText.text = questName;
        }

        public void SetVisible(bool setVisible)
        {
            visible = setVisible;
            if(setVisible)
                Show();
            else
                Hide();
        }

        private void Show() =>
            container.DOFade(1, DURATION).SetEase(Ease.InOutQuad);

        private void Hide() =>
            container.DOFade(0, DURATION).SetEase(Ease.InOutQuad);
    }
}
