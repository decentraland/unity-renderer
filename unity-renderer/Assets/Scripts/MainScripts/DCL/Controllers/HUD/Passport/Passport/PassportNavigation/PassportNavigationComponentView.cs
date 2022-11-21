using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentView : BaseComponentView, IPassportNavigationComponentView
    {
        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private GameObject wearablesPanel;

        private void EnableAboutPanel()
        {
            wearablesPanel.SetActive(false);
            aboutPanel.SetActive(true);
        }

        private void EnableWearablesPanel()
        {
            aboutPanel.SetActive(false);
            wearablesPanel.SetActive(true);
        }

        public override void RefreshControl()
        {
        }

    }
}