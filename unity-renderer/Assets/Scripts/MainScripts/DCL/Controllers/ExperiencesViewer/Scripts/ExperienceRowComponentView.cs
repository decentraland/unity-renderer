using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ExperiencesViewer
{
    public interface IExperienceRowComponentView
    {
        /// <summary>
        /// Event that will be triggered when the Show/Hide PEX UI button is clicked.
        /// </summary>
        event Action<string, bool> onShowPEXUI;

        /// <summary>
        /// Event that will be triggered when the Start(Stop PEX button is clicked.
        /// </summary>
        event Action<string, bool> onStartPEX;

        /// <summary>
        /// Set the PEX id.
        /// </summary>
        /// <param name="id">A string</param>
        void SetId(string id);

        /// <summary>
        /// Set an icon image from an uri.
        /// </summary>
        /// <param name="uri">Url of the icon image.</param>
        void SetIcon(string uri);

        /// <summary>
        /// Set the name label.
        /// </summary>
        /// <param name="name">A string.</param>
        void SetName(string name);

        /// <summary>
        /// Set the PEX UI as visible or not.
        /// </summary>
        /// <param name="isPlaying">True for set the PEX UI as visible.</param>
        void SetUIVisibility(bool isVisible);

        /// <summary>
        /// Set the PEX as playing or not.
        /// </summary>
        /// <param name="isPlaying">True for set it as playing.</param>
        void SetAsPlaying(bool isPlaying);

        /// <summary>
        /// Set the background color of the row.
        /// </summary>
        /// <param name="color">Color to apply.</param>
        void SetRowColor(Color color);

        /// <summary>
        /// Set the background color of the row when it is hovered.
        /// </summary>
        /// <param name="color">Color to apply.</param>
        void SetOnHoverColor(Color color);

        /// <summary>
        /// Set the ability of start/stop the experience (with a toggle) or only stop it (with a stop button).
        /// </summary>
        /// <param name="isAllowed">True for allowing it.</param>
        void SetAllowStartStop(bool isAllowed);
    }

    public class ExperienceRowComponentView : BaseComponentView, IExperienceRowComponentView, IComponentModelConfig<ExperienceRowComponentModel>
    {
        [Header("Prefab References")]
        [SerializeField] internal ImageComponentView iconImage;
        [SerializeField] internal ImageComponentView defaultIconImage;
        [SerializeField] internal TMP_Text nameText;
        [SerializeField] internal ShowHideAnimator showHideUIButtonsContainerAnimator;
        [SerializeField] internal ButtonComponentView showPEXUIButton;
        [SerializeField] internal ButtonComponentView hidePEXUIButton;
        [SerializeField] internal ToggleComponentView startStopPEXToggle;
        [SerializeField] internal ButtonComponentView stopPEXButton;
        [SerializeField] internal Image backgroundImage;

        [Header("Configuration")]
        [SerializeField] internal ExperienceRowComponentModel model;

        public event Action<string, bool> onShowPEXUI;
        public event Action<string, bool> onStartPEX;

        internal Color originalBackgroundColor;
        internal Color onHoverColor;

        public override void Awake()
        {
            base.Awake();

            originalBackgroundColor = backgroundImage.color;
            ConfigureRowButtons();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            SetUIVisibility(model.isUIVisible);
            SetAsPlaying(model.isPlaying);
        }

        public void Configure(ExperienceRowComponentModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetId(model.id);
            SetIcon(model.iconUri);
            SetName(model.name);
            SetUIVisibility(model.isUIVisible);
            SetAsPlaying(model.isPlaying);
            SetRowColor(model.backgroundColor);
            SetOnHoverColor(model.onHoverColor);
            SetAllowStartStop(model.allowStartStop);
        }

        public override void OnFocus()
        {
            base.OnFocus();

            backgroundImage.color = onHoverColor;
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();

            backgroundImage.color = originalBackgroundColor;
        }

        public void SetId(string id)
        {
            model.id = id;
        }

        public void SetIcon(string uri)
        {
            model.iconUri = uri;

            if (iconImage == null)
                return;

            if (!String.IsNullOrEmpty(uri))
            {
                iconImage.gameObject.SetActive(true);
                iconImage.SetImage(uri);
                defaultIconImage.gameObject.SetActive(false);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
                defaultIconImage.gameObject.SetActive(true);
            }
        }

        public void SetName(string name)
        {
            model.name = name;

            if (nameText == null)
                return;

            nameText.text = name;
        }

        public void SetUIVisibility(bool isVisible)
        {
            model.isUIVisible = isVisible;

            if (showPEXUIButton != null)
            {
                if (isVisible)
                    showPEXUIButton.Hide();
                else
                    showPEXUIButton.Show();
            }

            if (hidePEXUIButton != null)
            {
                if (isVisible)
                    hidePEXUIButton.Show();
                else
                    hidePEXUIButton.Hide();
            }
        }

        public void SetAsPlaying(bool isPlaying)
        {
            model.isPlaying = isPlaying;

            if (startStopPEXToggle != null && startStopPEXToggle.gameObject.activeSelf)
                startStopPEXToggle.isOn = isPlaying;

            if (stopPEXButton != null && stopPEXButton.gameObject.activeSelf && !isPlaying)
                stopPEXButton.gameObject.SetActive(false);

            if (showHideUIButtonsContainerAnimator != null)
            {
                if (isPlaying)
                    showHideUIButtonsContainerAnimator.Show();
                else
                    showHideUIButtonsContainerAnimator.Hide();
            }
        }

        public void SetRowColor(Color color)
        {
            model.backgroundColor = color;

            if (backgroundImage == null)
                return;

            backgroundImage.color = color;
            originalBackgroundColor = color;
        }

        public void SetOnHoverColor(Color color)
        {
            model.onHoverColor = color;
            onHoverColor = color;
        }

        public void SetAllowStartStop(bool isAllowed)
        {
            model.allowStartStop = isAllowed;

            if (startStopPEXToggle != null)
                startStopPEXToggle.gameObject.SetActive(isAllowed);

            if (stopPEXButton != null)
                stopPEXButton.gameObject.SetActive(!isAllowed);
        }

        public override void Dispose()
        {
            base.Dispose();

            showPEXUIButton?.onClick.RemoveAllListeners();
            hidePEXUIButton?.onClick.RemoveAllListeners();
            stopPEXButton?.onClick.RemoveAllListeners();
        }

        internal void ConfigureRowButtons()
        {
            showPEXUIButton?.onClick.AddListener(() =>
            {
                SetUIVisibility(true);
                onShowPEXUI?.Invoke(model.id, true);
            });

            hidePEXUIButton?.onClick.AddListener(() =>
            {
                SetUIVisibility(false);
                onShowPEXUI?.Invoke(model.id, false);
            });

            if (startStopPEXToggle != null)
            {
                startStopPEXToggle.OnSelectedChanged += (isOn, id, name) =>
                {
                    SetAsPlaying(isOn);
                    onStartPEX?.Invoke(model.id, isOn);
                };
            }

            stopPEXButton?.onClick.AddListener(() =>
            {
                SetAsPlaying(false);
                onStartPEX?.Invoke(model.id, false);
            });
        }
    }
}