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
    [SerializeField] internal TextMeshProUGUI coordsTxt;
    [SerializeField] internal TextMeshProUGUI parcelsAmountTxt;

    [SerializeField] internal Button selectButton;

    [SerializeField] internal GameObject disabledImage;
    [SerializeField] internal GameObject selectedImage;

    internal LandWithAccess land;
    internal AdapterState currentState;

    public AdapterState GetState() => currentState;
    public LandWithAccess GetLand() => land;

    private void Awake() { selectButton.onClick.AddListener(ItemSelected); }

    private void OnDestroy() { selectButton.onClick.RemoveAllListeners(); }

    public void SetContent(LandWithAccess land, AdapterState state)
    {
        this.land = land;

        landNameTxt.text = land.name;
        coordsTxt.text = BIWUtils.Vector2INTToString(land.baseCoords);
        parcelsAmountTxt.text = land.parcels.Length + " parcels";

        SetState(state);
    }

    public void SetState(AdapterState state)
    {
        currentState = state;

        selectedImage.SetActive(false);
        disabledImage.SetActive(false);

        switch (state)
        {
            case AdapterState.ENABLE:
                disabledImage.SetActive(false);
                break;
            case AdapterState.DISABLE:
                disabledImage.SetActive(true);
                break;
        }

    }

    public void ItemSelected() { OnLandSelected?.Invoke(land); }
}