using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DCL.LoadingScreen.V2
{
    /// <summary>
    /// View responsible of showing the corresponding provided Hint by LoadingScreenHintsController
    /// </summary>
    public class HintView : MonoBehaviour, IHintView
    {
        [SerializeField] internal TMP_Text hintTitleText;
        [SerializeField] internal TMP_Text hintBodyText;
        [SerializeField] internal Image hintImage;

        public void Initialize(Hint hint, Texture2D texture, bool startAsActive = false)
        {
            try
            {
                if (hintTitleText == null)
                    throw new System.Exception("HintView - HintText is not assigned!");
                if (hintTitleText == null)
                    throw new System.Exception("HintView - HintBodyText is not assigned!");
                if (hintImage == null)
                    throw new System.Exception("HintView - HintImage is not assigned!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                throw;
            }

            hintTitleText.text = hint.Title;
            hintBodyText.text = hint.Body;

            if (texture != null)
                hintImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            ToggleHint(startAsActive);
        }

        public void ToggleHint(bool active)
        {
            if (this != null)
            {
                transform.localPosition = Vector3.zero;
                gameObject.SetActive(active);
            }
        }
    }
}
