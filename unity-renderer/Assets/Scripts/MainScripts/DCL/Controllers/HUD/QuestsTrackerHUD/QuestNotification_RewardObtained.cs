using System.Collections;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestNotification_RewardObtained : MonoBehaviour, IQuestNotification
    {
        [SerializeField] internal TextMeshProUGUI rewardName;
        [SerializeField] internal RawImage rewardImage;
        [SerializeField] internal Button okButton;
        [SerializeField] internal AudioEvent rewardObtainedAudioEvent;

        private AssetPromise_Texture imagePromise;
        private bool okPressed = false;

        private void Awake() { okButton.onClick.AddListener(() => okPressed = true); }

        public void Populate(QuestReward reward)
        {
            this.rewardName.text = reward.name;
            SetImage(reward.imageUrl);
        }

        internal void SetImage(string itemImageURL)
        {
            if (imagePromise != null)
            {
                imagePromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(imagePromise);
            }

            if (string.IsNullOrEmpty(itemImageURL))
                return;

            imagePromise = new AssetPromise_Texture(itemImageURL);
            imagePromise.OnSuccessEvent += OnImageReady;
            imagePromise.OnFailEvent += x => { Debug.Log($"Error downloading reward image: {itemImageURL}"); };

            AssetPromiseKeeper_Texture.i.Keep(imagePromise);
        }

        private void OnImageReady(Asset_Texture assetTexture) { rewardImage.texture = assetTexture.texture; }

        public void Show()
        {
            gameObject.SetActive(true);
            rewardObtainedAudioEvent.Play();
            Utils.UnlockCursor();
        }

        public void Dispose() { Destroy(gameObject); }
        public IEnumerator Waiter() { yield return new WaitUntil(() => okPressed); }
    }
}