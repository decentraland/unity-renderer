using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using ThreeDISevenZeroR.UnityGifDecoder;
using ThreeDISevenZeroR.UnityGifDecoder.Model;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class UniGifStressTest : MonoBehaviour
{
    public string path = "";
    public bool isFile = false;
    public bool multithreaded = false;
    [SerializeField] private Image image;

    private  CancellationTokenSource source = new CancellationTokenSource();
    public Sprite[] sprites;
    public float[] delays;
    private double lastUpdate;
    private float currentDelay;
    private int currentFrameIndex;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadGifsOther().Forget();
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
    private async UniTask LoadGifsOther()
    {
        DataStore.i.performance.multithreading.Set(multithreaded);
        string dataPath = Application.dataPath;

        GifDecoderProcessor decoder;
        if (isFile)
        {
            var stream = File.OpenRead($"{dataPath}/{path}");
            decoder = new GifDecoderProcessor(stream);
        }
        else
        {
            decoder = new GifDecoderProcessor(path, WebRequestController.Create());
        }


        await decoder.Load(OnLoadComplete, _ => { }, source.Token);
    }
    private void OnLoadComplete(GifFrameData[] obj)
    {
        sprites = obj.Select(f => Sprite.Create(f.texture, new Rect(0, 0, f.texture.width, f.texture.height), Vector2.zero)).ToArray();
        delays = obj.Select(f => f.delay).ToArray();

        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,  sprites[0].texture.width);
        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sprites[0].texture.height);
    }

    private void OnDestroy() { Cancel(); }
    private void Cancel()
    {
        source.Cancel();
        source.Dispose();
        source = new CancellationTokenSource();
    }
}
