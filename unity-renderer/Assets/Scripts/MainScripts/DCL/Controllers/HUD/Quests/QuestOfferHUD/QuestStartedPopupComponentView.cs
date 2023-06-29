using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading;

namespace DCL.Quests
{
    public class QuestStartedPopupComponentView : BaseComponentView<QuestStartedPopupComponentModel>, IQuestStartedPopupComponentView
    {
        private const float FADE_DURATION = 0.167f;
        private const float SHOW_HUD_TIME = 10f;

        [SerializeField] internal TMP_Text questNameText;
        [SerializeField] internal Button openQuestLogButton;
        [SerializeField] internal CanvasGroup container;

        public event Action OnOpenQuestLog;
        internal bool visible = false;
        private CancellationTokenSource cts;

        public override void Awake()
        {
            openQuestLogButton.onClick.RemoveAllListeners();
            openQuestLogButton.onClick.AddListener(()=>
            {
                SetVisible(false);
                OnOpenQuestLog?.Invoke();
            });
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

            if (setVisible)
            {
                Show();
                cts?.Cancel();
                cts?.Dispose();
                cts = new CancellationTokenSource();
                WaitAndHide(cts).Forget();
            }
            else
                Hide();
        }

        private void Show() =>
            container.DOFade(1, FADE_DURATION).SetEase(Ease.InOutQuad);

        private void Hide() =>
            container.DOFade(0, FADE_DURATION).SetEase(Ease.InOutQuad);

        private async UniTaskVoid WaitAndHide(CancellationTokenSource ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(SHOW_HUD_TIME), cancellationToken: cts.Token);
            Hide();
        }

        public override void Dispose()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}
