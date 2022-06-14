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

        if (Input.GetKey(KeyCode.AltGr) &&  Input.GetKey(KeyCode.T))
        {
            activated = true;
            Log("Starting");
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
            Log($"Loaded: {textures.Count}");
            await UniTask.Delay(250);
        }
    }

    public void Log(string s)
    {
        bool logEnabled = Debug.unityLogger.logEnabled;
        Debug.unityLogger.logEnabled = true;
        Debug.Log(s);
        Debug.unityLogger.logEnabled = logEnabled;

    }
}