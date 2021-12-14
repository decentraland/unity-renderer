using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Builder
{
    public interface IPublishProjectController
    {
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
        private const string DETAIL_PREFAB_PATH = "Project/PublishDetailView";
        private const string PROGRESS_PREFAB_PATH = "Project/PublishProgressView";
        private const string SUCCESS_PREFAB_PATH = "Project/PublishSuccessView";

        private const string GENERIC_DEPLOY_ERROR = "Error deploying the scene";

        public enum ProjectPublishState
        {
            IDLE = 0,
            PUBLISH_CONFIRM = 1,
            PUBLISH_IN_PROGRESS = 2,
            PUBLISH_END = 3
        }

        internal IPublishProjectDetailView detailView;
        internal IPublishProjectProgressView progressView;
        internal IPublishProjectSuccesView succesView;

        internal ProjectPublishState projectPublishState = ProjectPublishState.IDLE;
        internal BuilderScene sceneToPublish;

        private PublishInfo.ProjectRotation projectRotation = PublishInfo.ProjectRotation.NORTH;

        public PublishProjectController()
        {
            detailView = GameObject.Instantiate(Resources.Load<PublishProjectDetailView>(DETAIL_PREFAB_PATH));
            progressView = GameObject.Instantiate(Resources.Load<PublishProjectProgressView>(PROGRESS_PREFAB_PATH));
            succesView = GameObject.Instantiate(Resources.Load<PublishProjectSuccesView>(SUCCESS_PREFAB_PATH));
            Initialize();
        }

        public void Initialize()
        {
            detailView.OnCancel += ViewClosed;
            detailView.OnPublishButtonPressed += PublishButtonPressedButtonPressed;
            detailView.OnProjectRotateLeft += RotateLeft;
            detailView.OnProjectRotateRight += RotateRight;

            progressView.OnViewClosed += ViewClosed;
            progressView.OnPublishConfirmButtonPressed += StartPublish;

            succesView.OnViewClose += ViewClosed;
        }

        public void Dispose()
        {
            detailView.OnCancel -= ViewClosed;
            detailView.OnPublishButtonPressed -= PublishButtonPressedButtonPressed;
            detailView.OnProjectRotateLeft -= RotateLeft;
            detailView.OnProjectRotateRight -= RotateRight;

            progressView.OnViewClosed -= ViewClosed;
            progressView.OnPublishConfirmButtonPressed -= StartPublish;

            succesView.OnViewClose -= ViewClosed;

            detailView.Dispose();
            progressView.Dispose();
            succesView.Dispose();
        }

        internal void RotateLeft()
        {
            projectRotation--;
            if (projectRotation < 0)
                projectRotation = PublishInfo.ProjectRotation.WEST;
        }

        internal void RotateRight()
        {
            projectRotation++;
            if (projectRotation > PublishInfo.ProjectRotation.WEST)
                projectRotation = PublishInfo.ProjectRotation.NORTH;
        }

        public void StartPublishFlow(IBuilderScene builderScene)
        {
            if (projectPublishState != ProjectPublishState.IDLE)
                return;

            //Note: in a future PR we will use IBuilderScene directly
            sceneToPublish = (BuilderScene) builderScene;
            projectPublishState = ProjectPublishState.PUBLISH_CONFIRM;

            detailView.SetProjectToPublish(sceneToPublish);
            detailView.Show();
        }

        private void ViewClosed()
        {
            projectPublishState = ProjectPublishState.IDLE;

            detailView.Hide();
            progressView.Hide();
            succesView.Hide();
        }

        private void PublishButtonPressedButtonPressed(PublishInfo info)
        {
            info.rotation = projectRotation;
            progressView.ShowConfirmPopUp();
        }

        private void StartPublish()
        {
            projectPublishState = ProjectPublishState.PUBLISH_IN_PROGRESS;

            progressView.PublishStarted();

            Promise<bool> deploymentPromise = DeployScene(sceneToPublish);
            deploymentPromise.Then(success =>
            {
                if (success)
                    DeploySuccess();
                else
                    DeployError(GENERIC_DEPLOY_ERROR);
            });
            deploymentPromise.Catch(DeployError);
        }

        private void DeploySuccess()
        {
            projectPublishState = ProjectPublishState.PUBLISH_END;
            progressView.Hide();
            succesView.ProjectPublished(sceneToPublish);
        }

        private void DeployError(string error)
        {
            projectPublishState = ProjectPublishState.PUBLISH_END;
            progressView.PublishError(error);
        }

        Promise<bool> DeployScene(BuilderScene scene)
        {
            Promise<bool> scenePromise = new Promise<bool>();

            //TODO: We need to implement the deployment functionality, for now we just mocked a delay to test the functionality 
            CoroutineStarter.Start(MockedDelay(scenePromise));
            return scenePromise;
        }

        // This will be deleted in the future, this just simulate a deploy after 3 seconds to test functionality
        IEnumerator MockedDelay(Promise<bool> promise)
        {
            yield return new WaitForSeconds(3f);
            promise.Resolve(true);
        }
    }
}