using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickBarSlot : MonoBehaviour
{
    [SerializeField] internal RawImage image;
    [SerializeField] internal TMP_Text text;
    [SerializeField] internal CanvasGroup canvasGroup;

    public bool isEmpty => !image.enabled;
    public Transform slotTransform => transform;

    public void SetTexture(Texture texture)
    {
        Texture2D copy = new Texture2D(texture.width, texture.height, texture.graphicsFormat, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
        Graphics.CopyTexture(texture, copy);
        image.texture = copy;
        image.enabled = true;
    }

    public void SetEmpty() { image.enabled = false; }

    public void EnableDragMode()
    {
        text.enabled = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }
}