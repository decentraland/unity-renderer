using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public class SearchLandView : BaseComponentView
    {
        public event Action<string> OnValueSearch;
        public event Action OnSearchCanceled;
        
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button cancelSearchButton;
        
        public override void RefreshControl() {  }
        
        public override void Start()
        {
            base.Start();
            inputField.onValueChanged.AddListener(InputChanged);
            cancelSearchButton.onClick.AddListener(ClearSearch);
        }

        public override void Dispose()
        {
            base.Dispose();
            inputField.onValueChanged.RemoveAllListeners();
        }

        public void ClearSearch()
        {
            inputField.SetTextWithoutNotify("");
            cancelSearchButton.gameObject.SetActive(false);
            OnSearchCanceled?.Invoke();
        }
        
        internal void InputChanged(string newValue)
        {
            if (string.IsNullOrEmpty(newValue) || newValue.Length < 2)
            {
                cancelSearchButton.gameObject.SetActive(false);
                return;
            }

            cancelSearchButton.gameObject.SetActive(true);
            OnValueSearch?.Invoke(newValue);
        }
    }
}
