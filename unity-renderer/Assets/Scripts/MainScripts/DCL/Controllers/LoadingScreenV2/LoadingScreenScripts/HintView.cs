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
    public class HintView : MonoBehaviour, IHintView
    {
        [SerializeField] internal TMP_Text hintText;
        [SerializeField] internal Image hintImage;

        public void Initialize(Hint hint, Texture2D texture, bool startAsActive = false)
        {
            try
            {
                if (hintText == null)
                    throw new System.Exception("HintView - HintText is not assigned!");
                if (hintImage == null)
                    throw new System.Exception("HintView - HintImage is not assigned!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                throw;
            }

            hintText.text = hint.Title;

            if (texture != null)
                hintImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            ToggleHint(startAsActive);
        }

        public void ToggleHint(bool active)
        {
            if (this != null)
                gameObject.SetActive(active);
        }
    }
}
