using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public interface IPublishProjectProgressView
    {
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
        /// Dispose the view
        /// </summary>
        void Dispose();
    }

    public class PublishProjectProgressView : BaseComponentView, IPublishProjectProgressView
    {
        public event Action OnPublishConfirmButtonPressed;
        public event Action OnViewClosed;

        [SerializeField] internal Button closeButton;
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button publishButton;
        [SerializeField] internal TMP_Text errorTextView;

        [SerializeField] internal ModalComponentView modal;

        [SerializeField] internal GameObject errorGameObject;
        [SerializeField] internal GameObject confirmGameObject;
        [SerializeField] internal GameObject progressGameObject;

        [SerializeField] internal LoadingBar loadingBar;

        private float currentProgress = 0;

        private Coroutine fakeProgressCoroutine;

        public override void Awake()
        {
            base.Awake();
            closeButton.onClick.AddListener(Close);
            cancelButton.onClick.AddListener(Close);
            publishButton.onClick.AddListener(ConfirmPublish);
        }

        public override void Dispose()
        {
            base.Dispose();
            closeButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            publishButton.onClick.RemoveAllListeners();

            if (fakeProgressCoroutine != null)
                StopCoroutine(fakeProgressCoroutine);
        }

        public override void RefreshControl() {  }

        public void ShowConfirmPopUp()
        {
            modal.Show();
            errorGameObject.SetActive(false);
            confirmGameObject.SetActive(true);
            progressGameObject.SetActive(false);
        }

        public void Close()
        {
            modal.Hide();
            OnViewClosed?.Invoke();
        }

        public void ConfirmPublish() { OnPublishConfirmButtonPressed?.Invoke(); }

        public void PublishStarted()
        {
            currentProgress = 0;

            Show();
            loadingBar.SetActive(true);
            errorGameObject.SetActive(false);
            confirmGameObject.SetActive(false);
            if (fakeProgressCoroutine != null)
                StopCoroutine(fakeProgressCoroutine);
            fakeProgressCoroutine = StartCoroutine(FakePublishProgress());
            AudioScriptableObjects.enable.Play();
        }

        public void Hide() { modal.Hide(); }

        public void PublishError(string message)
        {
            loadingBar.SetActive(false);
            errorGameObject.SetActive(true);
            confirmGameObject.SetActive(false);

            errorTextView.text = message;
        }

        public void SetPercentage(float newValue) { loadingBar.SetPercentage(newValue); }

        private IEnumerator FakePublishProgress()
        {
            while (true)
            {
                float newPercentage = Mathf.Clamp(
                    currentProgress + UnityEngine.Random.Range(1f, 10f),
                    currentProgress,
                    99f);

                SetPercentage(newPercentage);

                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
            }
        }
    }
}