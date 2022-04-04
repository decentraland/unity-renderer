using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeSceneHandler : MonoBehaviour
{
    const string PLAYER_PREFS_HOME_SCENE = "HomeScene";

    [SerializeField] public TextMeshProUGUI coordinatesText;
    [SerializeField] public Button saveCoordinatesButton;

    ParcelCoordinates currentHomeCoordinates;

    void Start()
    {
        currentHomeCoordinates = CoordinateUtils.ParseCoordinatesString(PlayerPrefsUtils.GetString("HomeScene", "0,0"));
        Debug.Log(currentHomeCoordinates.ToString());
        saveCoordinatesButton.onClick.AddListener(SetNewHomePoint);
    }

    internal void SetNewHomePoint()
    {
        if (coordinatesText.text == null)
            return;

        ParcelCoordinates newCoords = CoordinateUtils.ParseCoordinatesString(coordinatesText.text);

        if (!CoordinateUtils.IsCoordinateInRange(newCoords.x, newCoords.y) || newCoords.Equals(currentHomeCoordinates))
            return;

        currentHomeCoordinates = newCoords;
        SaveCoordinates();
    }

    private void SaveCoordinates()
    {
        PlayerPrefsUtils.SetString(PLAYER_PREFS_HOME_SCENE, currentHomeCoordinates.ToString());
        PlayerPrefsUtils.Save();
    }

    private void OnDestroy() 
    { 
        saveCoordinatesButton.onClick.RemoveAllListeners(); 
    }
}
