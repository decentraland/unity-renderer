using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using DCL.Interface;
using UnityEngine.EventSystems;
using System.Linq;

public class GotoPanel : MonoBehaviour
{
    [SerializeField] private Button teleportButton;
    [SerializeField] private TextMeshProUGUI panelText;
    [SerializeField] public GameObject container;

    private ParcelCoordinates targetCoordinates;

    private void Awake()
    {
        teleportButton.onClick.RemoveAllListeners();
        teleportButton.onClick.AddListener(TeleportTo);
    }

    private void TeleportTo()
    {
        WebInterface.GoTo(targetCoordinates.x, targetCoordinates.y);
        container.SetActive(false);
    }

    public void SetPanelText(ParcelCoordinates parcelCoordinates) 
    {
        targetCoordinates = parcelCoordinates;
        panelText.text = $"{parcelCoordinates.x},{parcelCoordinates.y}";
    }

}
