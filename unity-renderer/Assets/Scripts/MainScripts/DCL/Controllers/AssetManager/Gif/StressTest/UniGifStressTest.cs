using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class UniGifStressTest : MonoBehaviour
{
    public string path = "";
    public bool multithreaded = false;
    [SerializeField] private Image image;

    private  CancellationTokenSource source = new CancellationTokenSource();
    public Sprite[] sprites;
    private float[] delays;
    private double lastUpdate;
    private float currentDelay;
    private int currentFrameIndex;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadGifs().Forget();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Cancelled");
            Cancel();
            sprites = null;
        }

        if (sprites?.Length > 0)
        {
            if (Time.time - lastUpdate > currentDelay)
            {
                image.sprite = sprites[currentFrameIndex];
                currentFrameIndex = (currentFrameIndex + 1) % sprites.Length;
                lastUpdate = Time.time;
                currentDelay = delays[currentFrameIndex];
            }
        }
    }
    private async UniTask LoadGifs()
    {
        sprites = null;
        DataStore.i.performance.multithreading.Set(multithreaded);

        Debug.Log("Reading file");

        byte[] bytes = null;
        string dataPath = Application.dataPath;

        await TaskUtils.Run( () =>
        {
            bytes = File.ReadAllBytes($"{dataPath}/{path}");
        });

        Debug.Log("Starting Load");

        CancellationToken cancellationToken = source.Token;
        if (multithreaded)
        {
            await TaskUtils.Run( async () =>
            {
                await UniGif.GetTextureListAsync(bytes, OnLoadFinished, FilterMode.Bilinear, token: cancellationToken);
            }, cancellationToken: cancellationToken);
        }
        else
        {
            await UniGif.GetTextureListAsync(bytes, OnLoadFinished, FilterMode.Bilinear, token: cancellationToken);
        }
        
    }
    private void OnLoadFinished(GifFrameData[] texturelist, int animationloops, int width, int height)
    {
        Debug.Log("Ended load");
        Debug.Log($"Width {width}");
        Debug.Log($"Height {height}");
        Rect rect = new Rect(0, 0, width, height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);

        sprites = texturelist.Select(g => Sprite.Create(g.texture, rect, pivot))
                             .ToArray();
        delays = texturelist.Select(g => g.delay).ToArray();

        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void OnDestroy() { Cancel(); }
    private void Cancel()
    {
        source.Cancel();
        source.Dispose();
        source = new CancellationTokenSource();
    }
}