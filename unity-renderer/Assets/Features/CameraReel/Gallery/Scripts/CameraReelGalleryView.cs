﻿using Cysharp.Threading.Tasks;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CameraReel.Gallery
{
    public class CameraReelGalleryView : MonoBehaviour
    {
        private readonly Dictionary<int, GridContainerComponentView> monthContainers = new ();

        [SerializeField] internal RectTransform container;
        [SerializeField] private Button showMoreButton;
        [SerializeField] private RectTransform showMoreButtonPanel;
        [SerializeField] public Canvas canvas;

        [Header("RESOURCES")]
        [SerializeField] internal GameObject monthHeaderPrefab;
        [SerializeField] internal GridContainerComponentView monthGridContainerPrefab;
        [SerializeField] internal Image thumbnailPrefab;

        internal GridContainerComponentView currentMonthGridContainer;

        public event Action<CameraReelResponse> ScreenshotThumbnailClicked;
        public event Action ShowMoreButtonClicked;

        private void Awake()
        {
            SwitchVisibility(isVisible: false);
        }

        private void OnEnable()
        {
            showMoreButton.onClick.AddListener(() => ShowMoreButtonClicked?.Invoke());
        }

        private void OnDisable()
        {
            showMoreButton.onClick.RemoveAllListeners();
        }

        public void SwitchVisibility(bool isVisible) =>
            canvas.enabled = isVisible;

        public void DownloadImageAndCreateObject(List<CameraReelResponse> reelImages)
        {
            foreach (CameraReelResponse reel in reelImages)
            {
                int month = reel.metadata.GetLocalizedDateTime().Month;

                GridContainerComponentView gridContainer;

                if (!monthContainers.ContainsKey(month))
                {
                    GameObject separator = Instantiate(monthHeaderPrefab, container);
                    separator.gameObject.SetActive(true);
                    gridContainer = Instantiate(monthGridContainerPrefab, container);
                    gridContainer.gameObject.SetActive(true);

                    showMoreButtonPanel.SetAsLastSibling();

                    monthContainers.Add(month, gridContainer);
                }
                else
                    gridContainer = monthContainers[month];

                Image image = Instantiate(thumbnailPrefab, gridContainer.transform);
                image.gameObject.SetActive(true);

                Button button = image.GetComponent<Button>();
                button.onClick.AddListener(() => ScreenshotThumbnailClicked?.Invoke(reel));

                SetThumbnailFromWebAsync(reel, image);
            }
        }

        private static async Task SetThumbnailFromWebAsync(CameraReelResponse reel, Image image)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(reel.thumbnailUrl);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.Log(request.error);
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
