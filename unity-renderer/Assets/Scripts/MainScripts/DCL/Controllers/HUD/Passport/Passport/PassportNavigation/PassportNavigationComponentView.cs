using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassportNavigationComponentView : MonoBehaviour, IPassportNavigationComponentView
{
    [SerializeField] private GameObject aboutPanel;
    [SerializeField] private GameObject wearablesPanel;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button wearablesButton;

    public void Start()
    {
        aboutButton.onClick.AddListener(EnableAboutPanel);
        wearablesButton.onClick.AddListener(EnableWearablesPanel);
    }

    private void EnableAboutPanel()
    {
        aboutButton.image.enabled = false;
        wearablesButton.image.enabled = true;
        wearablesPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    private void EnableWearablesPanel()
    {
        aboutButton.image.enabled = true;
        wearablesButton.image.enabled = false;
        aboutPanel.SetActive(false);
        wearablesPanel.SetActive(true);
    }

}
