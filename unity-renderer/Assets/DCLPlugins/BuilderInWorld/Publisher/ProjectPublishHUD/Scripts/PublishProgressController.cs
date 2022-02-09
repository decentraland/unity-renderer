using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface IPublishProgressController
    {
        /// <summary>
        /// Fired when the user confirm the deployment
        /// </summary>
        event Action OnConfirm;

        /// <summary>
        /// The back button has been pressed
        /// </summary>
        event Action OnBackPressed;

        void Initialize();

        void Dispose();

        /// <summary>
        /// This will show the confirmation pop up
        /// </summary>
        void ShowConfirmDeploy();

        /// <summary>
        /// This is called when the deployment has succeed
        /// </summary>
        /// <param name="publishedScene"></param>
        void DeploySuccess();

        /// <summary>
        /// Set the scene to publish
        /// </summary>
        /// <param name="scene"></param>
        void SetInfoToPublish(IBuilderScene scene, PublishInfo info);

        /// <summary>
        /// This is called when the deployment has failed with the error as parameter
        /// </summary>
        /// <param name="error"></param>
        void DeployError(string error);
    }

    public class PublishProgressController : IPublishProgressController
    {
        public event Action OnConfirm;
        public event Action OnBackPressed;

        private const string PROGRESS_PREFAB_PATH = "PublishProgressView";

        internal IPublishProgressView view;

        public void Initialize()
        {
            view = GameObject.Instantiate(Resources.Load<PublishProgressView>(PROGRESS_PREFAB_PATH));

            view.OnViewClosed += ViewClosed;
            view.OnBackPressed += BackPressed;
            view.OnPublishConfirmButtonPressed += ConfirmPressed;
        }

        public void Dispose()
        {
            view.OnViewClosed -= ViewClosed;
            view.OnBackPressed -= BackPressed;
            view.OnPublishConfirmButtonPressed -= ConfirmPressed;

            view.Dispose();
        }

        private void BackPressed()
        {
            OnBackPressed?.Invoke();
        }
        
        public void ShowConfirmDeploy() { view.ConfirmDeployment(); }

        private void ConfirmPressed()
        {
            OnConfirm?.Invoke();
            view.PublishStarted();
        }

        public void DeploySuccess() { view.ProjectPublished(); }

        public void SetInfoToPublish(IBuilderScene scene, PublishInfo info) { view.SetPublishInfo(scene, info); }

        public void DeployError(string error) { view.PublishError(error); }

        private void ViewClosed() { view.Hide(); }
    }
}