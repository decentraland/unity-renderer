using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public interface IToast
    {
        /// <summary>
        /// Show the text
        /// </summary>
        /// <param name="instant"></param>
        void Show(bool instant);
        
        /// <summary>
        /// Set the text and changes the size of the toast based on the text
        /// </summary>
        /// <param name="text"></param>
        void SetText(string text);
    }
    
    public class Toast : BaseComponentView, IToast
    {
        [Tooltip("Represented in milliseconds")]
        [SerializeField] internal int msToBeActive = 2000;
        [SerializeField] internal float sizePerCharacter = 8.8f;
        [SerializeField] internal Image toastImage;
        [SerializeField] internal TextMeshProUGUI toastText;

        private Coroutine hideCoroutine;
        
        public override void RefreshControl() {  }

        public void SetText(string text)
        {
            toastText.text = text;

            Vector2 size = toastImage.rectTransform.sizeDelta;
            size.x = text.Length * sizePerCharacter;
            toastImage.rectTransform.sizeDelta = size;
        }

        public override void Show(bool instant)
        {
            base.Show(instant);
            if(hideCoroutine != null)
                StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(WaitAndDeactivate());
        }

        IEnumerator WaitAndDeactivate()
        {
            yield return new WaitForSeconds(msToBeActive / 1000f);
            Hide(false);
        }
    }
}
