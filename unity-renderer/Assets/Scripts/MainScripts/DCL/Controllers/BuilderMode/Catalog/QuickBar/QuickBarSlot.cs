using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickBarSlot : MonoBehaviour
{
    [SerializeField] private RawImage image;

    public void SetTexture(Texture texture)
    {
        image.texture = texture;
        image.enabled = true;
    }

    public void SetEmpty()
    {
        image.enabled = false;
    }
}
