using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;
using Variables.RealmsInfo;

namespace DCLPlugins.RealmsPlugin
{
    /// <summary>
    /// Controller for the Jump to Home logic.
    /// Its used to chose the most populated realm to jump home to
    /// </summary>
    public class JumpToHomeController : MonoBehaviour
    {

        [SerializeField] private Button jumpButton;
        [SerializeField] private ShowHideAnimator showHideAnimator;
        [SerializeField] private RectTransform positionWithMiniMap;
        [SerializeField] private RectTransform positionWithoutMiniMap;

        private BaseCollection<RealmModel> realms => DataStore.i.realm.realmsInfo;
        private BaseVariable<bool> jumpHomeButtonVisible => DataStore.i.HUDs.jumpHomeButtonVisible;
        private BaseVariable<bool> minimapVisible => DataStore.i.HUDs.minimapVisible;
        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = jumpButton.GetComponent<RectTransform>();
            jumpButton.onClick.AddListener(GoHome);
            jumpHomeButtonVisible.OnChange += SetVisibility;
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
            WebInterface.JumpInHome(GetMostPopulatedRealm());
        }

        private string GetMostPopulatedRealm()
        {
            List<RealmModel> currentRealms = realms.Get().ToList();
            return currentRealms.OrderByDescending(e => e.usersCount).FirstOrDefault()?.serverName;
        }

        private void OnDestroy() { jumpButton.onClick.RemoveListener(GoHome); }
    }
}