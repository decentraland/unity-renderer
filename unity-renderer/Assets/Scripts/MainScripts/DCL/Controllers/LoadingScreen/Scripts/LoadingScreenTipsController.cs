using Cysharp.Threading.Tasks;
using DCLServices.StableDiffusionService;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Loading screen tips provider. The responsibility of this class is to create the loading tips and provide them as needed
    /// </summary>
    public class LoadingScreenTipsController
    {
        private readonly TimeSpan SHOWING_TIME_TIPS = TimeSpan.FromSeconds(5);

        private List<LoadingTip> defaultLoadingTips;
        private int currentRandomIndex;

        private Queue<LoadingTip> currentSceneLoadingTips;
        private IStableDiffusionService stableDiffusionService;
        private int currentSceneLoadingImage;

        private readonly LoadingScreenTipsView tipsView;
        private CancellationTokenSource disposeCts;


        public LoadingScreenTipsController(LoadingScreenTipsView tipsView, IStableDiffusionService stableDiffusionService = null)
        {
            this.tipsView = tipsView;

            //tipsView.gameObject.SetActive(false);

            defaultLoadingTips = tipsView.defaultTips;
            currentRandomIndex = Random.Range(0, defaultLoadingTips.Count);
            this.stableDiffusionService = stableDiffusionService;
            StartTips();
        }

        private async UniTask GenerateNewImages()
        {
            async UniTask GenerateNewImage()
            {
                Texture2D newImage = await stableDiffusionService.GetTexture(new TextToImageConfig());
                currentSceneLoadingTips.Enqueue(new LoadingTip("Generated Image", newImage));
            }

            while (true)
            {
                while (currentSceneLoadingTips.Count < 4)
                    await GenerateNewImage();
                await UniTask.Delay(1000, cancellationToken: disposeCts.Token);
            }
        }

        //TODO: We will use this method when the WORLDs loads downloaded images and tips. This will come with the
        // AboutResponse var on the RealmPlugin change
        public void LoadCustomTips(List<Tuple<string, Sprite>> customList)
        {
            defaultLoadingTips = new List<LoadingTip>();

            foreach (Tuple<string, Sprite> tipsTuple in customList)
                defaultLoadingTips.Add(new LoadingTip(tipsTuple.Item1, tipsTuple.Item2));

            currentRandomIndex = Random.Range(0, defaultLoadingTips.Count);
        }

        public LoadingTip GetNextSceneLoadingTip()
        {
            return currentSceneLoadingTips.Dequeue();
        }

        public LoadingTip GetNextLoadingTip()
        {
            //We select a random value that is not the current one
            int randomIndexCandidate = Random.Range(0, defaultLoadingTips.Count);

            if (randomIndexCandidate.Equals(currentRandomIndex))
                randomIndexCandidate = (currentRandomIndex + 1) % defaultLoadingTips.Count;

            currentRandomIndex = randomIndexCandidate;
            return defaultLoadingTips[currentRandomIndex];
        }

        private async UniTask IterateTipsAsync()
        {
            while (true)
            {
                tipsView.ShowTip(currentSceneLoadingTips.Count > 0 ? GetNextSceneLoadingTip() : GetNextLoadingTip());
                await UniTask.Delay(SHOWING_TIME_TIPS, cancellationToken: disposeCts.Token);
            }
        }

        public void StopTips()
        {
            //This means that tips have been already stopped once. We only show them once, so we return and ignore this function
            if (disposeCts ==  null) return;

            tipsView.gameObject.SetActive(false);
            currentSceneLoadingTips = null;
            disposeCts.Cancel();
            disposeCts.Dispose();
            disposeCts = null;
        }

        public void StartTips()
        {
            if (disposeCts != null) return;

            disposeCts = new CancellationTokenSource();
            currentSceneLoadingTips = new Queue<LoadingTip>();
            currentSceneLoadingImage = 0;
            IterateTipsAsync();
            GenerateNewImages();
        }
    }

    [Serializable]
    public class LoadingTip
    {
        public string text;
        public Sprite sprite;

        public LoadingTip(string text, string spriteURL)
        {
            this.text = text;
            sprite = Resources.Load<Sprite>(spriteURL);
        }

        public LoadingTip(string text, Sprite sprite)
        {
            this.text = text;
            this.sprite = sprite;
        }

        public LoadingTip(string text, Texture2D texture)
        {
            this.text = text;
            this.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);;
        }
    }
}
