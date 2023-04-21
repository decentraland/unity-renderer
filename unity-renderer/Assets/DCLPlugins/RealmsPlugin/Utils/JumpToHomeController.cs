using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

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

        private BaseVariable<bool> jumpHomeButtonVisible => DataStore.i.HUDs.jumpHomeButtonVisible;
        private BaseVariable<bool> minimapVisible => DataStore.i.HUDs.minimapVisible;
        private BaseVariable<bool> exitedThroughButton => DataStore.i.common.exitedWorldThroughGoBackButton;

        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = jumpButton.GetComponent<RectTransform>();
            jumpButton.onClick.AddListener(GoHome);

            SetVisibility(jumpHomeButtonVisible.Get(), false);
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

        private void OnDestroy() =>
            jumpButton.onClick.RemoveListener(GoHome);
    }
}
