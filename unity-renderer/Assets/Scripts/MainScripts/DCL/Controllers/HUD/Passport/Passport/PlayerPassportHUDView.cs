using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPassportHUDView : MonoBehaviour
{

    [SerializeField] internal Button hideCardButton;
    [SerializeField] internal GameObject container;

    public static PlayerPassportHUDView CreateView()
    {
        return Instantiate(Resources.Load<GameObject>("PlayerPassport")).GetComponent<PlayerPassportHUDView>();
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetPassportPanelVisibility(bool visible)
    {
        container.SetActive(visible);
    }
}
