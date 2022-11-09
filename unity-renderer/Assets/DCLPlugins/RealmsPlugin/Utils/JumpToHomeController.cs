using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;
using Variables.RealmsInfo;

public class JumpToHomeController : MonoBehaviour
{

    [SerializeField] private Button jumpButton;
    [SerializeField]private ShowHideAnimator showHideAnimator;
    [SerializeField] private RectTransform positionWithMiniMap;
    [SerializeField] private RectTransform positionWithoutMiniMap;

    private BaseCollection<RealmModel> realms => DataStore.i.realm.realmsInfo;
    private BaseVariable<bool> jumpHomeButtonVisible => DataStore.i.HUDs.jumpHomeButtonVisible;
    private BaseVariable<bool> minimapVisible => DataStore.i.HUDs.minimapVisible;
    private RectTransform rectTransform;

    private void Start()
    {
        jumpButton.onClick.AddListener(GoHome);
        jumpHomeButtonVisible.OnChange += SetVisibility;
        rectTransform = jumpButton.GetComponent<RectTransform>();
    }
    
    private void SetVisibility(bool current, bool _)
    {
        if (current)
        {
            jumpButton.interactable = true;
            rectTransform.anchoredPosition = minimapVisible.Get() ? positionWithMiniMap.anchoredPosition : positionWithoutMiniMap.anchoredPosition;
            showHideAnimator.Show();
        }
        else
        {
            showHideAnimator.Hide();
        }
    }
    private void GoHome()
    {
        jumpButton.interactable = false;
        WebInterface.JumpIn(0, 0, GetMostPopulatedRealm(), "");
    }

    private string GetMostPopulatedRealm()
    {
        List<RealmModel> currentRealms = realms.Get().ToList();
        return currentRealms.OrderByDescending(e => e.usersCount).FirstOrDefault()?.serverName;
    }

    private void OnDestroy()
    {
        jumpButton.onClick.RemoveListener(GoHome);
    }

}