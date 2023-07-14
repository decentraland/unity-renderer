using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileLinkListComponentView : MonoBehaviour
    {
        [SerializeField] private Button addButton;
        [SerializeField] private TMP_InputField newLinkTitle;
        [SerializeField] private TMP_InputField newLinkUrl;
        [SerializeField] private MyProfileLinkComponentView linkPrefab;
        [SerializeField] private RectTransform linksContainer;

        private readonly List<MyProfileLinkComponentView> links = new ();
        private readonly Regex httpRegex = new ("^((https?:)?\\/\\/)?([\\da-z.-]+)\\.([a-z.]{2,6})([\\/\\w .%@()\\-]*)*\\/?$");

        private bool isAddEnabled = true;

        public event Action<(string title, string url)> OnAddedNew;
        public event Action<(string title, string url)> OnRemoved;

        private void Awake()
        {
            addButton.onClick.AddListener(() =>
            {
                if (!newLinkUrl.text.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && !newLinkUrl.text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    newLinkUrl.text = $"https://{newLinkUrl.text}";

                OnAddedNew?.Invoke((
                    title: newLinkTitle.text,
                    url: UnityWebRequest.EscapeURL(newLinkUrl.text)));
            });

            newLinkTitle.onValueChanged.AddListener(str => EnableOrDisableAddButton());
            newLinkUrl.onValueChanged.AddListener(str => EnableOrDisableAddButton());

            EnableOrDisableAddButton();
        }

        public void Add(string title, string url)
        {
            MyProfileLinkComponentView linkComponent = Instantiate(linkPrefab, linksContainer);
            linkComponent.Set(title, UnityWebRequest.UnEscapeURL(url));
            linkComponent.OnRemoved += OnRemoved;
            links.Add(linkComponent);
        }

        public void Clear()
        {
            foreach (MyProfileLinkComponentView linkComponent in links)
            {
                linkComponent.OnRemoved -= OnRemoved;
                Destroy(linkComponent.gameObject);
            }

            links.Clear();
        }

        public void ClearInput()
        {
            newLinkTitle.text = "";
            newLinkUrl.text = "";
        }

        public void EnableOrDisableAddNewLinkOption(bool enabled)
        {
            isAddEnabled = enabled;
            EnableOrDisableAddButton();
        }

        private void EnableOrDisableAddButton()
        {
            addButton.interactable = newLinkTitle.text.Length > 0 && newLinkUrl.text.Length > 0
                                                                  && httpRegex.IsMatch(newLinkUrl.text)
                                                                  && isAddEnabled;
        }
    }
}
