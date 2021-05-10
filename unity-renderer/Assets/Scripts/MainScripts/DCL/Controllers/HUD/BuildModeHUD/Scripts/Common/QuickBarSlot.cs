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
        Texture2D original = texture as Texture2D;
        Texture2D copy = new Texture2D(original.width, original.height, original.format, false);
        Graphics.CopyTexture(original, 0, 0, 0, 0, original.width, original.height, copy, 0, 0, 0, 0);

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