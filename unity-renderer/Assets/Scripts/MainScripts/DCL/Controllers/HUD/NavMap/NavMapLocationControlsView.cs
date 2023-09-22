using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class NavMapLocationControlsView : MonoBehaviour, INavMapLocationControlsView
    {
        [field: SerializeField] internal Button homeButton;
        [field: SerializeField] internal Button centerToPlayerButton;

        [Space]
        [SerializeField] private GameObject homeButtonRoot;
        [SerializeField] private GameObject centerToPlayerButtonRoot;
        public event Action HomeButtonClicked;
        public event Action CenterToPlayerButtonClicked;

        private void OnEnable()
        {
            homeButton.onClick.AddListener(() => HomeButtonClicked?.Invoke());
            centerToPlayerButton.onClick.AddListener(() => CenterToPlayerButtonClicked?.Invoke());
        }

        private void OnDisable()
        {
            homeButton.onClick.RemoveAllListeners();
            centerToPlayerButton.onClick.RemoveAllListeners();
        }

        public void Hide()
        {
            homeButtonRoot.SetActive(false);
            centerToPlayerButtonRoot.SetActive(false);
        }
    }
}
