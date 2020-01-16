using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsHUD
{
    public class ImageColorOnInteractable : MonoBehaviour
    {
        [SerializeField] Selectable targetSelectable;
        [SerializeField] Graphic targetGraphic;
        [SerializeField] Color interactableColor;
        [SerializeField] Color notInteractableColor;

        bool prevState;

        void Awake()
        {
            if (targetSelectable)
            {
                prevState = targetSelectable.interactable;
            }
        }

        void Update()
        {
            if (targetSelectable == null)
            {
                return;
            }

            if (prevState != targetSelectable.interactable)
            {
                prevState = targetSelectable.interactable;
                ApplyColor(prevState);
            }
        }

        void ApplyColor(bool interactable)
        {
            if (targetGraphic)
            {
                targetGraphic.color = interactable ? interactableColor : notInteractableColor;
            }
        }
    }
}