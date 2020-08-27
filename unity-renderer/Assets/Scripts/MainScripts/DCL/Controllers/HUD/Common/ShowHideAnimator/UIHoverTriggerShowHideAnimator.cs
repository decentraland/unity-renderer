using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverTriggerShowHideAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] ShowHideAnimator showHideAnimator = null;
    [SerializeField] bool hideAnimatorOnAwake = false;
    [SerializeField] bool enableAnimatorOnHover = false;

    void Awake()
    {
        if (hideAnimatorOnAwake)
        {
            showHideAnimator.gameObject.SetActive(false);
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (enableAnimatorOnHover && !showHideAnimator.gameObject.activeSelf)
        {
            showHideAnimator.gameObject.SetActive(true);
        }
        showHideAnimator.Show();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        showHideAnimator.Hide();
    }
}
