using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileLinkComponentView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text urlLabel;
        [SerializeField] private Button removeButton;

        public event Action<(string title, string url)> OnRemoved;

        private void Awake()
        {
            removeButton.onClick.AddListener(() => OnRemoved?.Invoke((titleLabel.text, urlLabel.text)));
        }

        public void Set(string title, string url)
        {
            titleLabel.text = title;
            urlLabel.text = url;
        }
    }
}
