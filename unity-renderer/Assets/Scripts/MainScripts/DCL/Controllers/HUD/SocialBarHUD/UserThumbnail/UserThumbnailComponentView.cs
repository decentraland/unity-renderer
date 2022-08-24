using UnityEngine;

namespace SocialBar.UserThumbnail
{
    public class UserThumbnailComponentView : BaseComponentView
    {
        [SerializeField] private ImageComponentView faceImage;
        [SerializeField] private GameObject onlineStatusContainer;
        [SerializeField] private GameObject offlineStatusContainer;
        [SerializeField] private GameObject blockedStatusContainer;

        [Header("Configuration")]
        [SerializeField] private UserThumbnailComponentModel model;

        public virtual void Configure(UserThumbnailComponentModel model)
        {
            this.model = model;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            faceImage.Configure(new ImageComponentModel {uri = model.faceUrl});
            onlineStatusContainer.SetActive(model.isOnline);
            offlineStatusContainer.SetActive(!model.isOnline);
            blockedStatusContainer.SetActive(model.isBlocked);
        }
    }
}