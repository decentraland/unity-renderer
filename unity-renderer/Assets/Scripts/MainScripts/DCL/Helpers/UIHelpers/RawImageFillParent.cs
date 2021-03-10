using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RawImage that resize itself to fill it parent while preserving it texture's aspect ratio.
///
/// Caution: This component doesn't work with UI atlases!
/// </summary>
public class RawImageFillParent : RawImage
{
    new public Texture texture
    {
        set
        {
            base.texture = value;

            if (value != null)
            {
                ResizeFillParent();
            }
        }
        get { return base.texture; }
    }

    protected override void Awake()
    {
        base.Awake();
        if (texture != null)
        {
            ResizeFillParent();
        }
    }

    void ResizeFillParent()
    {
        if (transform.parent == null)
        {
            return;
        }

        RectTransform parent = transform.parent as RectTransform;

        float h, w;
        h = parent.rect.height;
        w = h * (base.texture.width / (float) base.texture.height);

        if ((parent.rect.width - w) > 0)
        {
            w = parent.rect.width;
            h = w * (base.texture.height / (float) base.texture.width);
        }

        rectTransform.sizeDelta = new Vector2(w, h);
    }
}