using DCL;
using DCL.Interface;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Variables.RealmsInfo;

namespace DCLPlugins.RealmPlugin
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
        private BaseVariable<bool> exitedThroughButton => DataStore.i.common.exitedWorldThroughGoBackButton;
        private BaseVariable<bool> isWorld => DataStore.i.common.isWorld;

        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = jumpButton.GetComponent<RectTransform>();
            jumpButton.onClick.AddListener(GoHome);
            jumpHomeButtonVisible.OnChange += SetVisibility;
        }

        private void OnEnable()
        {
            isWorld.OnChange += OnIsWorldChanged;
        }

        private void OnDisable()
        {
            isWorld.OnChange -= OnIsWorldChanged;
        }

        private void OnIsWorldChanged(bool current, bool previous)
        {
            if (JumpedFromWorldToNotWorld())
                GoHome();

            bool JumpedFromWorldToNotWorld() => previous && current == false;
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
                showHideAnimator.Hide();
        }

        private void GoHome()
        {
            jumpButton.interactable = false;
            exitedThroughButton.Set(true);
            WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = "/goto home",
            });
        }

        private string GetMostPopulatedRealm()
        {
            var currentRealms = realms.Get().ToList();
            return currentRealms.OrderByDescending(e => e.usersCount).FirstOrDefault()?.serverName;
        }

        private void OnDestroy()
        {
            jumpButton.onClick.RemoveListener(GoHome);
        }
    }
}
