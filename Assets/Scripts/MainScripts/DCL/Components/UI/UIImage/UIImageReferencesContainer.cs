using DCL.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIImageReferencesContainer : UIReferencesContainer, IPointerDownHandler
    {
        [Header("UI Image Fields")]
        public HorizontalLayoutGroup paddingLayoutGroup;

        public RawImage image;
        public RectTransform imageRectTransform;

        bool VERBOSE = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            UIImage.Model ownerModel = owner.model as UIImage.Model;

            if (VERBOSE)
            {
                Debug.Log("pointer current raycast: " + eventData.pointerCurrentRaycast,
                    eventData.pointerCurrentRaycast.gameObject);
                Debug.Log("pointer press raycast: " + eventData.pointerPressRaycast,
                    eventData.pointerPressRaycast.gameObject);
            }

            if (!string.IsNullOrEmpty(ownerModel.onClick) &&
                eventData.pointerPressRaycast.gameObject == image.gameObject)
            {
                WebInterface.ReportOnClickEvent(owner.scene.sceneData.id, ownerModel.onClick);
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnEnterPressed();
            }
        }

        public void OnEnterPressed()
        {
            if (owner.model is UIImage.Model ownerModel && (!string.IsNullOrEmpty(ownerModel.onEnter) && ownerModel.visible))
            {
                WebInterface.ReportOnEnterEvent(owner.scene.sceneData.id, ownerModel.onEnter);
            }
        }
    }
}