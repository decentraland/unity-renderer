using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Altom.AltTester.UI
{

    public class AltPrefabDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var group = eventData.pointerDrag.AddComponent<CanvasGroup>();
                group.blocksRaycasts = false;

            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                eventData.pointerDrag.transform.position = Input.mousePosition;
#else
            eventData.pointerDrag.gameObject.transform.position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#endif
                var objectTranform = (RectTransform)eventData.pointerDrag.transform;
                if (objectTranform.position.x < objectTranform.rect.width / 2)
                {
                    objectTranform.position = new Vector3(objectTranform.rect.width / 2, objectTranform.position.y, objectTranform.position.z);
                }
                else if (objectTranform.position.x > Screen.width)
                {
                    objectTranform.position = new Vector3(Screen.width, objectTranform.position.y, objectTranform.position.z);
                }
                if (objectTranform.position.y < 0)
                {
                    objectTranform.position = new Vector3(objectTranform.position.x, 0, objectTranform.position.z);
                }
                else if (objectTranform.position.y > Screen.height - objectTranform.rect.height / 2)
                {
                    objectTranform.position = new Vector3(objectTranform.position.x, Screen.height - objectTranform.rect.height / 2, objectTranform.position.z);
                }

            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var canvasGroup = eventData.pointerDrag.GetComponent<CanvasGroup>();
                Destroy(canvasGroup);
            }
        }

    }

}