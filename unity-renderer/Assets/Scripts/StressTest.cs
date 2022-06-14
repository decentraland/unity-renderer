using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StressTest : MonoBehaviour
{
    [SerializeField] private Texture2D textureSrc;

    private bool activated = false;
    private void Update()
    {
        if (activated)
            return;

        if (Input.GetKey(KeyCode.RightControl) &&  Input.GetKey(KeyCode.T))
        {
            activated = true;
            DownloadImage();
        }
    }

    private async UniTaskVoid DownloadImage()
    {
        List<Texture2D> textures = new List<Texture2D>();
        while (true)
        {
            Texture2D texture = new Texture2D(textureSrc.width, textureSrc.height, TextureFormat.ARGB32, false);
            texture.SetPixels(textureSrc.GetPixels());
            texture.Apply(false, true);
            textures.Add(texture);
            await UniTask.Delay(250);
        }
    }
}