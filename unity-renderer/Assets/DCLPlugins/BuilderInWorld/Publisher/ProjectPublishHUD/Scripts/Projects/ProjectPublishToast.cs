using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectPublishToast : BaseComponentView
{   
    [SerializeField] internal TextMeshProUGUI landNameText;
    [SerializeField] internal TextMeshProUGUI landCoordText;
    [SerializeField] internal GameObject subTitleGameObject;
    
    public override void RefreshControl() {  }

    public void SetLandInfo(string landName, string landCoords)
    {
        landNameText.text = landName + " selected";
        landCoordText.text = landCoords;
    }

    public void SetSubtitleActive(bool isActive)
    {
        subTitleGameObject.SetActive(isActive);
    }
}
