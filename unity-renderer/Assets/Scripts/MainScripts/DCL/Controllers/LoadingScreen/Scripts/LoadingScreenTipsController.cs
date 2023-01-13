using Cysharp.Threading.Tasks;
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

        private List<LoadingTip> currentLoadingTips;
        private int currentRandomIndex;

        private readonly LoadingScreenTipsView tipsView;
        private CancellationTokenSource disposeCts;

        public LoadingScreenTipsController(LoadingScreenTipsView tipsView)
        {
            this.tipsView = tipsView;

            tipsView.gameObject.SetActive(false);

            currentLoadingTips = tipsView.defaultTips;
            currentRandomIndex = Random.Range(0, currentLoadingTips.Count);
        }

        //TODO: We will use this method when the WORLDs loads downloaded images and tips. This will come with the
        // AboutResponse var on the RealmPlugin change
        public void LoadCustomTips(List<Tuple<string, Sprite>> customList)
        {
            currentLoadingTips = new List<LoadingTip>();

            foreach (Tuple<string, Sprite> tipsTuple in customList)
                currentLoadingTips.Add(new LoadingTip(tipsTuple.Item1, tipsTuple.Item2));

            currentRandomIndex = Random.Range(0, currentLoadingTips.Count);
        }

        public LoadingTip GetNextLoadingTip()
        {
            //We select a random value that is not the current one
            int randomIndexCandidate = Random.Range(0, currentLoadingTips.Count);

            if (randomIndexCandidate.Equals(currentRandomIndex))
                randomIndexCandidate = (currentRandomIndex + 1) % currentLoadingTips.Count;

            currentRandomIndex = randomIndexCandidate;
            return currentLoadingTips[currentRandomIndex];
        }

        private async UniTask IterateTipsAsync()
        {
            while (true)
            {
                tipsView.ShowTip(GetNextLoadingTip());
                await UniTask.Delay(SHOWING_TIME_TIPS, cancellationToken: disposeCts.Token);
            }
        }

        public void StopTips()
        {
            //This means that tips have been already stopped once. We only show them once, so we return and ignore this function
            if (disposeCts ==  null) return;

            tipsView.gameObject.SetActive(false);
            disposeCts.Cancel();
            disposeCts.Dispose();
            disposeCts = null;
        }

        public void StartTips()
        {
            if (disposeCts != null) return;

            disposeCts = new CancellationTokenSource();
            IterateTipsAsync();
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
    }
}
