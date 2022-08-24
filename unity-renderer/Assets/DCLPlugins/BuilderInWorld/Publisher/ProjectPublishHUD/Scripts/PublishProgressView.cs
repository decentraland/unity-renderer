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
        /// This represent the current state of the publishing view
        /// </summary>
        enum PublishStatus
        {
            IDLE = 0,
            CONFIRM = 1,
            PUBLISHING = 2,
            ERROR = 3,
            SUCCESS = 4
        }
        
        /// <summary>
        /// This action will be called when the confirm button is pressed
        /// </summary>
        event Action OnPublishConfirmButtonPressed;

        /// <summary>
        /// This action will be called when the view is closed
        /// </summary>
        event Action OnViewClosed;

        /// <summary>
        /// Whe should go back to the previous view
        /// </summary>
        event Action OnBackPressed;

        /// <summary>
        /// Show the confirm publish pop up
        /// </summary>
        void ConfirmDeployment();

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
        /// Shows that the project has been successfully published
        /// </summary>
        void ProjectPublished();

        /// <summary>
        /// Sets the project to display
        /// </summary>
        /// <param name="publishedProject"></param>
        void SetPublishInfo(IBuilderScene publishedProject, PublishInfo info);

        /// <summary>
        /// Dispose the view
        /// </summary>
        void Dispose();
    }

    public class PublishProgressView : BaseComponentView, IPublishProgressView
    {
        // Main titles 
        private const string CONFIRM_MAIN_TEXT = "Are you sure you want to publish your project?";
        private const string ERROR_MAIN_TEXT = "The publication of your project has failed!";
        private const string SUCCESS_MAIN_TEXT = "Project successfully published!";
        private const string PUBLISHING_MAIN_TEXT = "Publishing";
        
        // Sub titles
        private const string CONFIRM_SUB_TITLE_TEXT =  @"{0} will be deployed in {1},{2}";
        private const string PUBLISHING_SUB_TITLE_TEXT =  @"{0} is being deployed in {1},{2}";
        private const string SUCCESS_SUB_TITLE_TEXT =  @"{0} is now a live place in [landName]{1},{2}";
        
        public event Action OnPublishConfirmButtonPressed;
        public event Action OnViewClosed;
        public event Action OnBackPressed;

        [Header("General references")]
        [SerializeField] internal TMP_Text mainTitleTextView;
        [SerializeField] internal TMP_Text subTitleTextView;
        [SerializeField] internal RawImage screenshotImage;
        [SerializeField] internal ModalComponentView modal;
        
        [Header("Confirm Popup references")]
        [SerializeField] internal GameObject confirmGameObject;
        [SerializeField] internal Button noButton;
        [SerializeField] internal Button yesButton;
        
        [Header("Publish Progress references")]
        [SerializeField] internal GameObject progressGameObject;
        [SerializeField] internal LoadingBar loadingBar;
        [Header("Error references")]
        [SerializeField] internal TMP_Text errorTextView;
        [SerializeField] internal GameObject errorGameObject;
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button retryButton;
        
        [Header("Success references")]
        [SerializeField] internal Button okButton;
        [SerializeField] internal GameObject successGameObject;
        private IPublishProgressView.PublishStatus currentStatus;
        private float currentProgress = 0;
        private Coroutine fakeProgressCoroutine;
        private IBuilderScene currentScene;
        private PublishInfo currentInfo;

        public override void RefreshControl() { }

        public override void Awake()
        {
            base.Awake();
            cancelButton.onClick.AddListener(Close);
            noButton.onClick.AddListener(Back);
            okButton.onClick.AddListener(Close);

            yesButton.onClick.AddListener(ConfirmPublish);
            retryButton.onClick.AddListener(ConfirmPublish);

            gameObject.SetActive(false);
            modal.OnCloseAction += Close;
        }

        public override void Dispose()
        {
            base.Dispose();
            cancelButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();
            yesButton.onClick.RemoveAllListeners();
            okButton.onClick.RemoveAllListeners();
            retryButton.onClick.RemoveAllListeners();

            if (fakeProgressCoroutine != null)
                StopCoroutine(fakeProgressCoroutine);
            
            modal.OnCloseAction -= Close;
        }

        private void ShowCurrentStatus()
        {
            successGameObject.SetActive(false);
            errorGameObject.SetActive(false);
            confirmGameObject.SetActive(false);
            progressGameObject.SetActive(false);
            modal.CanBeCancelled(true);
            
            switch (currentStatus)
            {
                case IPublishProgressView.PublishStatus.CONFIRM:
                    ShowConfirmPopUp();
                    break;
                case IPublishProgressView.PublishStatus.PUBLISHING:
                    ShowProgressView();
                    break;
                case IPublishProgressView.PublishStatus.ERROR:
                    ShowPublishError();
                    break;
                case IPublishProgressView.PublishStatus.SUCCESS:
                    ShowProjectPublishSucces();
                    break;
            }
        }

        public void SetPublishInfo(IBuilderScene publishedProject, PublishInfo info)
        {
            screenshotImage.texture = publishedProject.sceneScreenshotTexture;
            currentScene = publishedProject;
            currentInfo = info;
        }

        public void ProjectPublished() { ChangeStatus(IPublishProgressView.PublishStatus.SUCCESS); }

        internal string GetConfirmSubtitleText(string baseText)
        {
            string title = currentScene.manifest.project.title;
            string posX = currentInfo.coordsToPublish.x.ToString();
            string posY = currentInfo.coordsToPublish.y.ToString();

            return string.Format(baseText, title, posX, posY);
        }

        public void ShowConfirmPopUp()
        {
            confirmGameObject.SetActive(true);
            mainTitleTextView.text = CONFIRM_MAIN_TEXT;
            subTitleTextView.text = GetConfirmSubtitleText(CONFIRM_SUB_TITLE_TEXT);
            gameObject.SetActive(true);
            modal.Show();
        }

        public void ConfirmDeployment() { ChangeStatus(IPublishProgressView.PublishStatus.CONFIRM); }

        public void ShowProgressView()
        {
            mainTitleTextView.text = PUBLISHING_MAIN_TEXT;
            subTitleTextView.text = GetConfirmSubtitleText(PUBLISHING_SUB_TITLE_TEXT);
            
            currentProgress = 0;
            modal.CanBeCancelled(false);
            progressGameObject.SetActive(true);
            if (fakeProgressCoroutine != null)
                StopCoroutine(fakeProgressCoroutine);
            fakeProgressCoroutine = StartCoroutine(FakePublishProgress());
            AudioScriptableObjects.enable.Play();
        }

        public void ShowProjectPublishSucces()
        {
            mainTitleTextView.text = SUCCESS_MAIN_TEXT;
            string text = GetConfirmSubtitleText(SUCCESS_SUB_TITLE_TEXT);

            foreach (var land in DataStore.i.builderInWorld.landsWithAccess.Get())
            {
                if (land.baseCoords == currentInfo.coordsToPublish && !string.IsNullOrEmpty(land.name))
                    text = text.Replace("[landName]", land.name + " ");
            }
            
            text = text.Replace("[landName]", "");
            subTitleTextView.text = text;
            
            successGameObject.SetActive(true);
        }
        
        public void ShowPublishError()
        {
            mainTitleTextView.text = ERROR_MAIN_TEXT;
            errorGameObject.SetActive(true);
        }

        private void ChangeStatus(IPublishProgressView.PublishStatus status)
        {
            currentStatus = status;
            ShowCurrentStatus();
        }

        public void Back()
        {
            modal.Hide();
            OnBackPressed?.Invoke();
        }

        public void Close()
        {
            modal.Hide();
            OnViewClosed?.Invoke();
        }

        [ContextMenu("Start Deployment")]
        public void ConfirmPublish() { OnPublishConfirmButtonPressed?.Invoke(); }

        public void PublishStarted() { ChangeStatus(IPublishProgressView.PublishStatus.PUBLISHING); }

        public void Hide()
        {
            modal.Hide();
            if (fakeProgressCoroutine != null)
                StopCoroutine(fakeProgressCoroutine);
        }

        public void PublishError(string message)
        {
            errorTextView.text = message;
            ChangeStatus(IPublishProgressView.PublishStatus.ERROR);
        }

        public void SetPercentage(float newValue) { loadingBar.SetPercentage(newValue); }

        private IEnumerator FakePublishProgress()
        {
            while (true)
            {
                float newPercentage = UnityEngine.Random.Range(1f, 15f);
                currentProgress += newPercentage;

                currentProgress = Mathf.Clamp(
                    currentProgress,
                    currentProgress - newPercentage,
                    99f);

                SetPercentage(currentProgress);

                yield return new WaitForSeconds(UnityEngine.Random.Range(0.15f, 0.65f));
            }
        }
    }
}