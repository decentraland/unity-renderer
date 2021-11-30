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
        event Action OnClose;
        void Hide();
        void ProjectPublished(BuilderScene publishedProject);
        void Dispose();
    }

    public class PublishProjectSuccesView : BaseComponentView, IPublishProjectSuccesView
    {
        public event Action OnClose;

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

        public void Close() { OnClose?.Invoke(); }

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

            return title + " is now a live place in " + posX + "," + posY + " ready to be visited!";
        }
    }
}