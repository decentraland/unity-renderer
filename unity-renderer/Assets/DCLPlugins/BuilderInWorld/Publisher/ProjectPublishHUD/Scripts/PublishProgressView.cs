using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public interface IPublishProgressView
    {
        /// <summary>
        /// The publish has been confirmed
        /// </summary>
        event Action<IBuilderScene, PublishInfo> OnPublishPressed;
        
        /// <summary>
        /// This action will be called when the confirm button is pressed
        /// </summary>
        event Action OnPublishConfirmButtonPressed;

        /// <summary>
        /// This action will be called when the view is closed
        /// </summary>
        event Action OnViewClosed;

        /// <summary>
        /// Show the confirm publish pop up
        /// </summary>
        void ShowConfirmPopUp();

        /// <summary>
        /// Call this function when the publish start
        /// </summary>
        void PublishStarted();

        /// <summary>
        /// Hide the view
        /// </summary>
        void Hide();

        /// <summary>
        /// Shows the errors passes in a pop up
        /// </summary>
        /// <param name="message"></param>
        void PublishError(string message);

        /// <summary>
        /// Start the publish flow for a project
        /// </summary>
        /// <param name="builderScene"></param>
        void StartPublishFlow(IBuilderScene builderScene);

        /// <summary>
        /// Init the view
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Dispose the view
        /// </summary>
        void Dispose();
    }

    public class PublishProgressView : BaseComponentView
    {
        private const string SUB_TITLE_TEXT =  @"{0} is now a live place in {1},{2} ready to be visited!";

        public event Action OnViewClose;

        [SerializeField] internal Button okButton;
        [SerializeField] internal TMP_Text subTitleTextView;

        [SerializeField] internal RawImage screenshotImage;

        [SerializeField] internal ModalComponentView modal;

        public override void RefreshControl() { }

        public override void Awake()
        {
            base.Awake();
            okButton.onClick.AddListener(OkButtonPressed);
            modal.OnCloseAction += Close;

            gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            base.Dispose();
            okButton.onClick.RemoveAllListeners();
            modal.OnCloseAction -= Close;
        }

        internal void OkButtonPressed()
        {
            Hide();
            Close();
        }

        public void Close() { OnViewClose?.Invoke(); }

        public void ProjectPublished(IBuilderScene publishedProject)
        {
            gameObject.SetActive(true);
            modal.Show();

            subTitleTextView.text = GetSubTitleText(publishedProject);
            screenshotImage.texture = publishedProject.sceneScreenshotTexture;
        }

        public void Hide() { modal.Hide(); }

        internal string GetSubTitleText(IBuilderScene scene)
        {
            string title = scene.manifest.project.title;
            string posX = scene.scene.sceneData.basePosition.x.ToString();
            string posY = scene.scene.sceneData.basePosition.y.ToString();

            return string.Format(SUB_TITLE_TEXT, title, posX, posY);
        }
    }
}