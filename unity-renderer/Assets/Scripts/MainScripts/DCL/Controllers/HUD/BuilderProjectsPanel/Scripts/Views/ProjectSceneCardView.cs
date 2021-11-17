using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Helpers;
using TMPro;
using UnityEngine;

namespace DCL.Builder
{


    internal interface IProjectSceneCardView : IDisposable
    {
        /// <summary>
        ///  Setting button pressed
        /// </summary>
        event Action<ProjectData> OnSettingsPressed;

        /// <summary>
        /// Data of the project card
        /// </summary>
        Scene scene { get; }

        /// <summary>
        /// Position of the context menu button
        /// </summary>
        Vector3 contextMenuButtonPosition { get; }

        /// <summary>
        /// This setup the scene data
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="outdated">If the scene is sync with the project</param>
        void Setup(Scene scene, bool outdated);
        
        /// <summary>
        /// Active the card
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);
    }

    public class ProjectSceneCardView : MonoBehaviour, IProjectSceneCardView
    {
        const int THMBL_MARKETPLACE_WIDTH = 196;
        const int THMBL_MARKETPLACE_HEIGHT = 143;
        const int THMBL_MARKETPLACE_SIZEFACTOR = 50;
        
        public event Action<ProjectData> OnSettingsPressed;

        public Scene scene => sceneData;
        public Vector3 contextMenuButtonPosition { get; }
        
        [Header("Design Variables")]
        [SerializeField] private  float animationSpeed = 6f;
        
        [Header("Project References")]
        [SerializeField] private Texture2D defaultThumbnail;
        
        [Header("Prefab references")]
        [SerializeField] private GameObject loadingImgGameObject;
        [SerializeField] internal GameObject outdatedGameObject;
        
        [SerializeField] internal TextMeshProUGUI sceneNameTxt;
        [SerializeField] internal TextMeshProUGUI sceneCoordsTxt;
        
        [SerializeField] internal CanvasGroup canvasGroup;
        
        [Space]
        [SerializeField] private RawImageFillParent thumbnail;

        private string thumbnailUrl;
        private AssetPromise_Texture thumbnailPromise;
        private Scene sceneData;
        
        public void Setup(Scene scene, bool outdated)
        {
            sceneData = scene;
            
            sceneNameTxt.text = scene.title;
            sceneCoordsTxt.text = scene.@base.x + "," + scene.@base.y;
            
            string sceneThumbnailUrl = scene.navmapThumbnail;
            if (string.IsNullOrEmpty(sceneThumbnailUrl) && scene.parcels != null)
            {
                sceneThumbnailUrl = MapUtils.GetMarketPlaceThumbnailUrl(scene.parcels,
                    THMBL_MARKETPLACE_WIDTH, THMBL_MARKETPLACE_HEIGHT, THMBL_MARKETPLACE_SIZEFACTOR);
            }
            
            SetThumbnail(sceneThumbnailUrl);
            outdatedGameObject.SetActive(outdated);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
        }

        public void SetActive(bool active)
        {
            float from = active ? 0 : 1;
            float to = active ? 1 : 0;
            CoroutineStarter.Start( SetActiveAnimation(from,to));
        }
        
        private void SetThumbnail(string thumbnailUrl)
        {
            if (this.thumbnailUrl == thumbnailUrl)
                return;

            this.thumbnailUrl = thumbnailUrl;
            
            if (thumbnailPromise != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
                thumbnailPromise = null;
            }

            if (string.IsNullOrEmpty(thumbnailUrl))
            {
                SetThumbnail((Texture2D) null);
                return;
            }

            loadingImgGameObject.SetActive(true);
            
            thumbnailPromise = new AssetPromise_Texture(thumbnailUrl);
            thumbnailPromise.OnSuccessEvent += texture => SetThumbnail(texture.texture);
            thumbnailPromise.OnFailEvent += texture => SetThumbnail((Texture2D) null);
            thumbnail.enabled = false;
            
            AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
        }

        private void SetThumbnail(Texture2D thumbnailTexture)
        {
            loadingImgGameObject.SetActive(false);
            thumbnail.texture = thumbnailTexture ?? defaultThumbnail;
            thumbnail.enabled = true;
        }
        
        IEnumerator SetActiveAnimation(float from, float to)
        {
            gameObject.SetActive(true);
            
            float time = 0;

            while (time < 1)
            {
                time += Time.deltaTime * animationSpeed;
                canvasGroup.alpha = Mathf.Lerp(from,to,time);
                yield return null;
            }
            gameObject.SetActive(to >= 0.99f);
        }
    }
}