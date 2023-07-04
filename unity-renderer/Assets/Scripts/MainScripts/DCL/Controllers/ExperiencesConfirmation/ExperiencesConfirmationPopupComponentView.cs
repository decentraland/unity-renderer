using System;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPopupComponentView : BaseComponentView<ExperiencesConfirmationViewModel>,
        IExperiencesConfirmationPopupView
    {
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button rejectButton;

        public event Action OnAccepted;
        public event Action OnRejected;

        public override void Awake()
        {
            base.Awake();

            acceptButton.onClick.AddListener(() => OnAccepted?.Invoke());
            rejectButton.onClick.AddListener(() => OnRejected?.Invoke());
        }

        public override void RefreshControl()
        {
        }

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            base.Show(instant);
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);

            if (instant)
                gameObject.SetActive(false);
        }
    }
}
