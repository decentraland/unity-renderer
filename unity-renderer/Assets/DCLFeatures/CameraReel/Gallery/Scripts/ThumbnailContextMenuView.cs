using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class ThumbnailContextMenuView : MonoBehaviour, IThumbnailContextMenuView
    {
        public static readonly BaseList<ThumbnailContextMenuView> Instances = new ();

        [SerializeField] private Button downloadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button copyLinkButton;
        [SerializeField] private Button shareToTwitterButton;
        [SerializeField] private bool selfDestroy;

        public event Action OnDownloadRequested;
        public event Action OnDeletePictureRequested;
        public event Action OnCopyPictureLinkRequested;
        public event Action OnShareToTwitterRequested;
        public event Action<CameraReelResponse> OnSetup;

        private void Awake()
        {
            downloadButton.onClick.AddListener(() =>
            {
                OnDownloadRequested?.Invoke();
                Hide();
            });
            deleteButton.onClick.AddListener(() =>
            {
                OnDeletePictureRequested?.Invoke();
                Hide();
            });
            copyLinkButton.onClick.AddListener(() =>
            {
                OnCopyPictureLinkRequested?.Invoke();
                Hide();
            });
            shareToTwitterButton.onClick.AddListener(() =>
            {
                OnShareToTwitterRequested?.Invoke();
                Hide();
            });

            Instances.Add(this);
        }

        private void OnDestroy()
        {
            Instances.Remove(this);
        }

        private void Update()
        {
            HideIfClickedOutside();
        }

        public void Show(CameraReelResponse picture)
        {
            gameObject.SetActive(true);
            OnSetup?.Invoke(picture);
        }

        public void Dispose()
        {
            if (selfDestroy)
                Destroy(gameObject);
        }

        private void Hide() =>
            gameObject.SetActive(false);

        private void HideIfClickedOutside()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            if (raycastResults.All(result => result.gameObject != downloadButton.gameObject
                && result.gameObject != deleteButton.gameObject
                && result.gameObject != copyLinkButton.gameObject
                && result.gameObject != shareToTwitterButton.gameObject))
                Hide();
        }
    }
}
