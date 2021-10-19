using DCL;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    [Serializable]
    internal class LeftMenuSettingsViewReferences
    {
        public TextMeshProUGUI titleLabel;
        public RawImageFillParent thumbnail;
        public GameObject coordsContainer;
        public GameObject sizeContainer;
        public GameObject entitiesCountContainer;
        public TextMeshProUGUI coordsText;
        public TextMeshProUGUI sizeText;
        public TextMeshProUGUI entitiesCountText;
        public GameObject avatarThumbnailContainer;
        public RawImage avatarImage;
        public TextMeshProUGUI authorNameText;
        public LeftMenuButtonToggleView adminsMenuToggle;
    }

    internal class LeftMenuSettingsViewHandler : IDisposable
    {
        internal const string PROJECT_TITLE = "Project settings";
        internal const string SCENE_TITLE = "Scene settings";

        private readonly LeftMenuSettingsViewReferences viewReferences;
        private readonly IPlacesViewController placesViewController;
        private readonly Texture defaultThumbnail;

        private AssetPromise_Texture promiseAvatarThumbnail;
        private AssetPromise_Texture promiseSceneThumbnail;

        public LeftMenuSettingsViewHandler(LeftMenuSettingsViewReferences viewReferences, IPlacesViewController placesViewController)
        {
            this.viewReferences = viewReferences;
            this.placesViewController = placesViewController;

            defaultThumbnail = viewReferences.thumbnail.texture;

            placesViewController.OnProjectSelected += SelectProject;
        }

        public void Dispose()
        {
            placesViewController.OnProjectSelected -= SelectProject;

            if (promiseAvatarThumbnail != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(promiseAvatarThumbnail);
            }
            if (promiseSceneThumbnail != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(promiseSceneThumbnail);
            }

            promiseAvatarThumbnail = null;
            promiseSceneThumbnail = null;
        }

        void SetProjectData(IPlaceData placeData)
        {
            viewReferences.titleLabel.text = placeData.isDeployed ? SCENE_TITLE : PROJECT_TITLE;
            viewReferences.coordsContainer.SetActive(placeData.isDeployed);
            viewReferences.sizeContainer.SetActive(!placeData.isDeployed);
            viewReferences.entitiesCountContainer.SetActive(!placeData.isDeployed);
            viewReferences.coordsText.text = $"{placeData.coords.x},{placeData.coords.y}";
            viewReferences.sizeText.text = $"{placeData.size.x},{placeData.size.y}m";
            viewReferences.entitiesCountText.text = placeData.entitiesCount.ToString();
            viewReferences.authorNameText.text = placeData.authorName;
            viewReferences.adminsMenuToggle.gameObject.SetActive(placeData.isDeployed);

            SetThumbnails(placeData);
        }

        void SetThumbnails(IPlaceData placeData)
        {
            if (string.IsNullOrEmpty(placeData.authorThumbnail))
            {
                viewReferences.avatarThumbnailContainer.SetActive(false);
            }
            else
            {
                if (promiseAvatarThumbnail != null)
                {
                    AssetPromiseKeeper_Texture.i.Forget(promiseAvatarThumbnail);
                }

                promiseAvatarThumbnail = new AssetPromise_Texture(placeData.authorThumbnail);
                promiseAvatarThumbnail.OnFailEvent += asset => viewReferences.avatarThumbnailContainer.SetActive(false);
                promiseAvatarThumbnail.OnSuccessEvent += asset =>
                {
                    viewReferences.avatarThumbnailContainer.SetActive(true);
                    viewReferences.avatarImage.texture = asset.texture;
                };
                AssetPromiseKeeper_Texture.i.Keep(promiseAvatarThumbnail);
            }

            if (string.IsNullOrEmpty(placeData.thumbnailUrl))
            {
                viewReferences.thumbnail.texture = defaultThumbnail;
            }
            else
            {
                if (promiseSceneThumbnail != null)
                {
                    AssetPromiseKeeper_Texture.i.Forget(promiseSceneThumbnail);
                }

                promiseSceneThumbnail = new AssetPromise_Texture(placeData.thumbnailUrl);
                promiseSceneThumbnail.OnFailEvent += asset => viewReferences.thumbnail.texture = defaultThumbnail;
                promiseSceneThumbnail.OnSuccessEvent += asset =>
                {
                    viewReferences.thumbnail.texture = asset.texture;
                };
                AssetPromiseKeeper_Texture.i.Keep(promiseSceneThumbnail);
            }
        }

        private void SelectProject(IPlaceCardView placeCardView) { SetProjectData(placeCardView.PlaceData); }
    }
}