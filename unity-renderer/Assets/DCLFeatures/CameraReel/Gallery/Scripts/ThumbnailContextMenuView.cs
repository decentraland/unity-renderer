using DCL.Helpers;
using DCLServices.CameraReelService;
using System;
using UIComponents.ContextMenu;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    [RequireComponent(typeof(RectTransform))]
    public class ThumbnailContextMenuView : ContextMenuComponentView, IThumbnailContextMenuView
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

        public override void Awake()
        {
            base.Awake();

            HidingHierarchyTransforms = new[]
            {
                transform,
                downloadButton.transform,
                deleteButton.transform,
                copyLinkButton.transform,
                shareToTwitterButton.transform,
            };

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

        public override void RefreshControl() { }

        public void Show(CameraReelResponse picture)
        {
            gameObject.SetActive(true);
            ClampPositionToScreenBorders(transform.position);
            OnSetup?.Invoke(picture);
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);

            gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            if (selfDestroy)
                Utils.SafeDestroy(gameObject);
        }
    }
}
