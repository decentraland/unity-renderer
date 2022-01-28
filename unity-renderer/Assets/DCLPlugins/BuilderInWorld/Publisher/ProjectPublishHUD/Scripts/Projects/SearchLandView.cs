using System;
using TMPro;
using UnityEngine;

namespace DCL.Builder
{
    public class SearchLandView : BaseComponentView
    {
        public event Action<string> OnValueSearch;
        [SerializeField] private TMP_InputField inputField;
        
        public override void RefreshControl() {  }
        
        public override void Start()
        {
            base.Start();
            inputField.onValueChanged.AddListener(InputChanged);
        }

        public override void Dispose()
        {
            base.Dispose();
            inputField.onValueChanged.RemoveAllListeners();
        }
        
        internal void InputChanged(string newValue)
        {
            if(string.IsNullOrEmpty(newValue) || newValue.Length < 2)
                return;
            OnValueSearch?.Invoke(newValue);
        }
    }
}
