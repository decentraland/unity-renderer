using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Controllers.HUD.SettingsPanelHUD
{
    /// <summary>
    /// Hide gameObject when user clicks outside of it
    /// </summary>
    public class OutsideClickHideComponent : MonoBehaviour, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool mouseIsOver;

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!mouseIsOver)
                gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseIsOver = true;
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseIsOver = false;
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
