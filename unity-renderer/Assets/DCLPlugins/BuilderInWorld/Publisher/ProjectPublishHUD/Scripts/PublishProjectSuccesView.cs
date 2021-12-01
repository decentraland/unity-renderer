using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public interface IPublishProjectSuccesView
    {
        /// <summary>
        /// Called when view is closed
        /// </summary>
        event Action OnViewClose;

        /// <summary>
        /// Hide the view
        /// </summary>
        void Hide();

        /// <summary>
        /// Shows the view with the project data
        /// </summary>
        /// <param name="publishedProject"></param>
        void ProjectPublished(BuilderScene publishedProject);

        /// <summary>
        /// Dispose the view
        /// </summary>
        void Dispose();
    }

    public class PublishProjectSuccesView : BaseComponentView, IPublishProjectSuccesView
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

        public void ProjectPublished(BuilderScene publishedProject)
        {
            modal.Show();

            subTitleTextView.text = GetSubTitleText(publishedProject);
            screenshotImage.texture = publishedProject.sceneScreenshotTexture;
        }

        public void Hide() { modal.Hide(); }

        internal string GetSubTitleText(BuilderScene scene)
        {
            string title = scene.manifest.project.title;
            string posX = scene.scene.sceneData.basePosition.x.ToString();
            string posY = scene.scene.sceneData.basePosition.y.ToString();

            return string.Format(SUB_TITLE_TEXT, title, posX, posY);
        }
    }
}