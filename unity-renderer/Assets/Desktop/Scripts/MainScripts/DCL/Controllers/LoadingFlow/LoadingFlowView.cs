using System;
using MainScripts.DCL.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MainScripts.DCL.Controllers.LoadingFlow
{
    public class LoadingFlowView : MonoBehaviour, ILoadingFlowView
    {
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            exitButton.onClick.AddListener(OnExit);
        }

        private void OnDestroy()
        {
            exitButton.onClick.RemoveListener(OnExit);
        }

        private void OnExit()
        {
            DesktopUtils.Quit();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }

    public interface ILoadingFlowView
    {
        void Hide();

        void Show();
    }
}
