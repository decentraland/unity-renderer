using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIImageReferencesContainer : UIReferencesContainer
    {
        [Header("UI Image Fields")]
        public HorizontalLayoutGroup paddingLayoutGroup;

        public RawImage image;
        public RectTransform imageRectTransform;

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