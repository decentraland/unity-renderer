using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Builder
{
    public interface IPublishProjectController
    {
        /// <summary>
        /// The publish has been confirmed
        /// </summary>
        event Action<IBuilderScene, PublishInfo> OnPublishPressed;

        /// <summary>
        /// Start the publish flow for a project
        /// </summary>
        /// <param name="builderScene"></param>
        void StartPublishFlow(IBuilderScene builderScene);

        void Initialize();
        void Dispose();
    }

    public class PublishProjectController : IPublishProjectController
    {
        public event Action<IBuilderScene, PublishInfo> OnPublishPressed;

        private const string DETAIL_PREFAB_PATH = "Project/PublishPopupView";

        internal IPublishProjectDetailView detailView;

        internal IBuilderScene sceneToPublish;

        private PublishInfo.ProjectRotation projectRotation = PublishInfo.ProjectRotation.NORTH;

        public PublishProjectController() { detailView = GameObject.Instantiate(Resources.Load<PublishProjectDetailView>(DETAIL_PREFAB_PATH)); }

        public void Initialize()
        {
            detailView.OnCancel += ViewClosed;
            detailView.OnPublishButtonPressed += PublishButtonPressedButtonPressed;
            detailView.OnProjectRotateChange += RotationChange;
        }

        public void Dispose()
        {
            detailView.OnCancel -= ViewClosed;
            detailView.OnPublishButtonPressed -= PublishButtonPressedButtonPressed;
            detailView.OnProjectRotateChange -= RotationChange;

            detailView.Dispose();
        }

        internal void RotationChange(PublishInfo.ProjectRotation newRotation) { projectRotation = newRotation; }

        public void StartPublishFlow(IBuilderScene builderScene)
        {
            sceneToPublish = builderScene;

            detailView.SetProjectToPublish(sceneToPublish);
            detailView.Show();
        }

        private void ViewClosed() { detailView.Hide(); }

        private void PublishButtonPressedButtonPressed(PublishInfo info)
        {
            info.rotation = projectRotation;
            OnPublishPressed?.Invoke(sceneToPublish, info);
        }
    }
}