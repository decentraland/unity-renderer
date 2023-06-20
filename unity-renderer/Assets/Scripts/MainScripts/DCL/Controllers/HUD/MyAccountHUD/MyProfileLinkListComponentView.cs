using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileLinkListComponentView : MonoBehaviour
    {
        [SerializeField] private Button addButton;
        [SerializeField] private InputField newLinkTitle;
        [SerializeField] private InputField newLinkUrl;
        [SerializeField] private MyProfileLinkComponentView linkPrefab;
        [SerializeField] private RectTransform linksContainer;

        private readonly List<MyProfileLinkComponentView> links = new ();
        private readonly Regex httpRegex = new (@"/^https?:\/\/(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&\/=]*)$/");

        public event Action<(string title, string url)> OnAddedNew;
        public event Action<(string title, string url)> OnRemoved;

        private void Awake()
        {
            addButton.onClick.AddListener(() =>
                OnAddedNew?.Invoke((title: newLinkTitle.text, url: newLinkUrl.text)));

            newLinkTitle.onValueChanged.AddListener(str => EnableOrDisableAddButton());

            EnableOrDisableAddButton();
        }

        public void Add(string title, string url)
        {
            MyProfileLinkComponentView linkComponent = Instantiate(linkPrefab, linksContainer);
            linkComponent.Set(title, url);
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

        private void EnableOrDisableAddButton()
        {
            addButton.interactable = newLinkTitle.text.Length > 0 && newLinkUrl.text.Length > 0
                                                                  && httpRegex.IsMatch(newLinkUrl.text);
        }
    }
}
