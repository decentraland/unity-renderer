using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PubllishLandListAdapter : MonoBehaviour
{
    public enum AdapterState
    {
        ENABLE = 0,
        DISABLE = 1
    }

    public event Action<LandWithAccess> OnLandSelected;

    [SerializeField] internal TextMeshProUGUI landNameTxt;

    [SerializeField] internal Button selectButton;

    internal LandWithAccess land;
    internal AdapterState currentState;

    public AdapterState GetState() => currentState;
    public LandWithAccess GetLand() => land;

    private void Awake() { selectButton.onClick.AddListener(ItemSelected); }

    private void OnDestroy() { selectButton.onClick.RemoveAllListeners(); }

    public void SetContent(LandWithAccess land, AdapterState state)
    {
        this.land = land;

        string text = land.name;
        string coordsText = BIWUtils.Vector2INTToString(land.baseCoords);
        if(!text.Contains(coordsText))
            text += " <color=#716B7C>"+coordsText+"</color>";
        
        landNameTxt.text = text;

        SetState(state);
    }

    public void SetState(AdapterState state)
    {
        currentState = state;
    }

    public void ItemSelected() { OnLandSelected?.Invoke(land); }
}