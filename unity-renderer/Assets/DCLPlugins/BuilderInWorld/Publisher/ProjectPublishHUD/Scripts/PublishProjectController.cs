using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Builder
{
    public interface IPublishProjectController
    {
        void StartPublishFlow(IBuilderScene builderScene);
    }

    public class PublishProjectController : IPublishProjectController
    {
        private const string DETAIL_PREFAB_PATH = "BuilderProjectsPanel";
        private const string PROGRESS_PREFAB_PATH = "BuilderProjectsPanel";
        private const string SUCCESS_PREFAB_PATH = "BuilderProjectsPanel";

        public enum ProjectPublishState
        {
            IDLE = 0,
            PUBLISH_CONFIRM = 1,
            PUBLISH_DEPLOYING = 2,
            PUBLISH_END = 3
        }

        internal IPublishProjectDetailView detailView;
        internal IPublishProjectProgressView progressView;
        internal IPublishProjectSuccesView succesView;

        private ProjectPublishState projectPublishState = ProjectPublishState.IDLE;
        internal BuilderScene sceneToPublish;

        public PublishProjectController()
        {
            detailView = Object.Instantiate(Resources.Load<PublishProjectDetailView>(DETAIL_PREFAB_PATH));
            progressView = Object.Instantiate(Resources.Load<PublishProjectProgressView>(PROGRESS_PREFAB_PATH));
            succesView = Object.Instantiate(Resources.Load<PublishProjectSuccesView>(SUCCESS_PREFAB_PATH));
            Initialize();
        }

        public void Initialize()
        {
            detailView.OnCancel += Close;
            detailView.OnPublish += PublishButtonPressed;

            progressView.OnClose += Close;
            progressView.OnPublishConfirm += Publish;

            succesView.OnClose += Close;
        }

        public void Dipose()
        {
            detailView.OnCancel -= Close;
            detailView.OnPublish -= PublishButtonPressed;

            progressView.OnClose -= Close;
            progressView.OnPublishConfirm -= Publish;

            succesView.OnClose -= Close;

            detailView.Dispose();
            progressView.Dispose();
            succesView.Dispose();
        }

        public void StartPublishFlow(IBuilderScene builderScene)
        {
            if (projectPublishState != ProjectPublishState.IDLE)
                return;

            sceneToPublish = (BuilderScene) builderScene;
            projectPublishState = ProjectPublishState.PUBLISH_CONFIRM;
            detailView.SetBuilderScene(sceneToPublish);
        }

        private void Close()
        {
            projectPublishState = ProjectPublishState.IDLE;

            detailView.Hide();
            progressView.Hide();
            succesView.Hide();
        }

        private void PublishButtonPressed() { progressView.ShowConfirmPopUp(); }

        private void Publish()
        {
            projectPublishState = ProjectPublishState.PUBLISH_DEPLOYING;

            progressView.PublishStart();

            Promise<bool> deploymentPromise = DeployScene(sceneToPublish);
            deploymentPromise.Then(success =>
            {
                if (success)
                    DeploySuccess();
                else
                    DeployError("Error deploying the scene");
            });
            deploymentPromise.Catch(DeployError);
        }

        private void DeploySuccess()
        {
            projectPublishState = ProjectPublishState.PUBLISH_END;
            progressView.PublishEnd(false, "");
            succesView.ProjectPublished(sceneToPublish);
        }

        private void DeployError(string error)
        {
            projectPublishState = ProjectPublishState.PUBLISH_END;
            progressView.PublishEnd(false, error);
        }

        Promise<bool> DeployScene(BuilderScene scene)
        {
            Promise<bool> scenePromise = new Promise<bool>();
            CoroutineStarter.Start(MockedDelay(scenePromise));
            return scenePromise;
        }

        IEnumerator MockedDelay(Promise<bool> promise)
        {
            yield return new WaitForSeconds(3f);
            promise.Resolve(true);
        }
    }
}