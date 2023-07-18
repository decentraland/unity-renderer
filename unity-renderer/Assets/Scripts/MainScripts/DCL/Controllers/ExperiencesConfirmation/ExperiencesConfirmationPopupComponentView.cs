using DCL.Helpers;
using System;
using TMPro;
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
        [SerializeField] private Button[] cancelButtons;
        [SerializeField] private InputAction_Trigger cancelTrigger;
        [SerializeField] private GameObject permissionsContainer;
        [SerializeField] private GameObject descriptionContainer;
        [SerializeField] private TMP_Text permissionsLabel;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private Toggle dontAskMeAgainToggle;
        [SerializeField] private ImageComponentView iconImage;
        [SerializeField] private Texture2D defaultIconSprite;
        [SerializeField] private RectTransform root;
        [SerializeField] private GameObject smartWearableTitle;
        [SerializeField] private GameObject scenePxTitle;

        public event Action OnAccepted;
        public event Action OnRejected;
        public event Action OnCancelled;
        public event Action OnDontShowAnymore;
        public event Action OnKeepShowing;

        public override void Awake()
        {
            base.Awake();

            acceptButton.onClick.AddListener(() => OnAccepted?.Invoke());
            rejectButton.onClick.AddListener(() => OnRejected?.Invoke());

            foreach (Button cancelButton in cancelButtons)
                cancelButton.onClick.AddListener(() => OnCancelled?.Invoke());

            dontAskMeAgainToggle.onValueChanged.AddListener(arg0 =>
            {
                if (arg0)
                    OnDontShowAnymore?.Invoke();
                else
                    OnKeepShowing?.Invoke();
            });
        }

        public override void OnEnable()
        {
            base.OnEnable();

            cancelTrigger.OnTriggered += OnCancelTriggered;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            cancelTrigger.OnTriggered -= OnCancelTriggered;
        }

        public override void RefreshControl()
        {
            bool isPermissionsEnabled = model.Permissions.Count > 0;
            bool isDescriptionEnabled = !string.IsNullOrEmpty(model.Description);

            permissionsContainer.SetActive(isPermissionsEnabled);
            descriptionContainer.SetActive(isDescriptionEnabled);

            permissionsLabel.text = "";
            foreach (string permission in model.Permissions)
                permissionsLabel.text += $"- {permission}\n";

            descriptionLabel.text = model.Description;
            nameLabel.text = model.Name;

            if (string.IsNullOrEmpty(model.IconUrl))
            {
                iconImage.UseLoadingIndicator = false;
                iconImage.SetImage(defaultIconSprite);
                iconImage.ImageComponent.color = new Color(0.44f, 0.41f, 0.48f, 1f);
            }
            else
            {
                iconImage.UseLoadingIndicator = true;
                iconImage.SetImage(model.IconUrl);
                iconImage.ImageComponent.color = Color.white;
            }

            smartWearableTitle.SetActive(model.IsSmartWearable);
            scenePxTitle.SetActive(!model.IsSmartWearable);

            root.ForceUpdateLayout();
        }

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            base.Show(instant);

            // let the subscribers know that the default option is 'keep showing'
            if (dontAskMeAgainToggle.isOn)
                dontAskMeAgainToggle.isOn = false;
            else
                OnKeepShowing?.Invoke();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);

            if (instant)
                gameObject.SetActive(false);
        }

        private void OnCancelTriggered(DCLAction_Trigger action) =>
            OnCancelled?.Invoke();
    }
}
