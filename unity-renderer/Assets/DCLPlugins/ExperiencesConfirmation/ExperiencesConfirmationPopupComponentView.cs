using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPopupComponentView : BaseComponentView<ExperiencesConfirmationViewModel>,
        IExperiencesConfirmationPopupView
    {
        [Serializable]
        private struct PermissionContainer
        {
            public string permission;
            public GameObject container;
        }

        [SerializeField] private Button acceptButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private Button[] cancelButtons;
        [SerializeField] private InputAction_Trigger cancelTrigger;
        [SerializeField] private GameObject permissionsContainer;
        [SerializeField] private GameObject descriptionContainer;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text allowButtonLabel;
        [SerializeField] private TMP_Text rejectButtonLabel;
        [SerializeField] private Toggle dontAskMeAgainToggle;
        [SerializeField] private ImageComponentView iconImage;
        [SerializeField] private Texture2D defaultIconSprite;
        [SerializeField] private RectTransform root;
        [SerializeField] private GameObject smartWearableTitle;
        [SerializeField] private GameObject scenePxTitle;
        [SerializeField] private List<PermissionContainer> permissionsConfig;
        [SerializeField] private Button useWeb3ApiInfoButton;
        [SerializeField] private ShowHideAnimator useWeb3ApiInfoToast;

        private Dictionary<string, GameObject> permissionContainers;

        public event Action OnAccepted;
        public event Action OnRejected;
        public event Action OnCancelled;
        public event Action OnDontShowAnymore;
        public event Action OnKeepShowing;

        public override void Awake()
        {
            base.Awake();

            permissionContainers = permissionsConfig.ToDictionary(container => container.permission, container => container.container);

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

            useWeb3ApiInfoButton.onClick.AddListener(() =>
            {
                if (useWeb3ApiInfoToast.gameObject.activeSelf)
                    useWeb3ApiInfoToast.Hide();
                else
                {
                    useWeb3ApiInfoToast.gameObject.SetActive(true);
                    useWeb3ApiInfoToast.ShowDelayHide(10f);
                }
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

            foreach ((string _, GameObject container) in permissionContainers)
                container.SetActive(false);

            foreach (string permission in model.Permissions)
                permissionContainers[permission].SetActive(true);

            descriptionLabel.text = model.Description;
            nameLabel.text = model.Name;

            if (string.IsNullOrEmpty(model.IconUrl))
            {
                iconImage.UseLoadingIndicator = false;
                iconImage.SetImage(defaultIconSprite);
                iconImage.ImageComponent.color = new Color(0.08627451f, 0.08235294f, 0.09411765f, 1f);
            }
            else
            {
                iconImage.UseLoadingIndicator = true;
                iconImage.SetImage(model.IconUrl);
                iconImage.ImageComponent.color = Color.white;
            }

            smartWearableTitle.SetActive(model.IsSmartWearable);
            scenePxTitle.SetActive(!model.IsSmartWearable);

            if (model.IsSmartWearable)
            {
                allowButtonLabel.text = "ALLOW AND EQUIP";
                rejectButtonLabel.text = "DON'T ALLOW\nAND EQUIP";
            }
            else
            {
                allowButtonLabel.text = "OK";
                rejectButtonLabel.text = "DON'T ALLOW";
            }

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
