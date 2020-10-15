using UnityEngine;
using TMPro;

internal class ManaHUDview : MonoBehaviour
{
    [SerializeField] internal ShowHideAnimator mainShowHideAnimator;
    [SerializeField] internal TextMeshProUGUI balanceText;
    [SerializeField] internal Button_OnPointerDown buttonManaInfo;
    [SerializeField] internal Button_OnPointerDown buttonManaPurchase;
    [SerializeField] internal UIHoverTriggerShowHideAnimator uiHoverTriggerShowHideAnimator;

    public void SetVisibility(bool visible)
    {
        if (visible && !mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Show();
        else if (!visible && mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Hide();
    }
}
