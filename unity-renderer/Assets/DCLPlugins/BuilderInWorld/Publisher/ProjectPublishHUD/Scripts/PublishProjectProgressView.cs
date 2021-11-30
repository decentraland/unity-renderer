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
        event Action OnPublishConfirm;
        event Action OnClose;

        void ShowConfirmPopUp();
        void PublishStart();

        void Hide();

        public void PublishEnd(bool isOk, string message);

        void Dispose();
    }

    public class PublishProjectProgressView : BaseComponentView, IPublishProjectProgressView
    {
        public event Action OnPublishConfirm;
        public event Action OnClose;

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
            OnClose?.Invoke();
        }

        public void ConfirmPublish() { OnPublishConfirm?.Invoke(); }

        public void PublishStart()
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

        public void PublishEnd(bool isOk, string message)
        {
            if (isOk)
            {
                Hide();
            }
            else
            {
                loadingBar.SetActive(false);
                errorGameObject.SetActive(true);
                confirmGameObject.SetActive(false);

                errorTextView.text = message;
            }
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