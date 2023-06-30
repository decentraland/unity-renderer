using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileAdditionalInfoEntryComponentView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField] private Button removeButton;

        private string id;

        public event Action<string> OnRemoved;

        private void Awake()
        {
            removeButton.onClick.AddListener(() => OnRemoved?.Invoke(id));
        }

        public void Set(string id, string title, string value)
        {
            this.id = id;
            titleLabel.text = title;
            valueLabel.text = value;
        }
    }
}
