using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    /// View responsible of showing the corresponding provided Hint by LoadingScreenHintsController
    /// </summary>
    public class HintView : MonoBehaviour
    {
        [SerializeField] internal TMP_Text hintText;
        [SerializeField] internal Image hintImage;

        public void Initialize(Hint hint, Texture2D texture)
        {
            hintText.text = hint.Title;
            hintImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            HideHint();
        }

        public void ShowHint()
        {
            gameObject.SetActive(true);
        }

        public void HideHint()
        {
            gameObject.SetActive(false);
        }

    }
}
