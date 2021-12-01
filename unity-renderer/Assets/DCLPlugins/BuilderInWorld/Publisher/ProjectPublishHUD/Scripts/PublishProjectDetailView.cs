using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public interface IPublishProjectDetailView
    {
        /// <summary>
        /// If the publish is canceled this action will be called
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// If the publish button is pressed this action will be called
        /// </summary>
        public event Action OnPublishButtonPressed;

        /// <summary>
        /// Set the project to publish
        /// </summary>
        /// <param name="scene"></param>
        void SetProjectToPublish(BuilderScene scene);

        /// <summary>
        /// This will show the detail modal 
        /// </summary>
        void Show();

        /// <summary>
        /// This will hide the detail modal
        /// </summary>
        void Hide();

        /// <summary>
        /// Dispose the view
        /// </summary>
        void Dispose();
    }

    public class PublishProjectDetailView : BaseComponentView, IPublishProjectDetailView
    {
        private const string SCREENSHOT_TEXT =  @"{0} parcel = {1}x{2}m";

        //TODO: This will be implemented in the future
        public event Action OnProjectRotate;
        public event Action OnCancel;
        public event Action OnPublishButtonPressed;

        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button publishButton;

        //TODO: This functionality will be implemented in the future
        [SerializeField] internal Button rotateLeftButton;
        [SerializeField] internal Button rotateRightButton;

        [SerializeField] internal Button zoomInButton;
        [SerializeField] internal Button zoomOutButton;
        [SerializeField] internal Button resetToLandButton;
        [SerializeField] internal Image mapImage;

        [SerializeField] internal RawImage sceneScreenshotImage;

        [SerializeField] internal ModalComponentView modal;

        [SerializeField] internal TMP_Text sceneScreenshotParcelText;
        [SerializeField] internal LimitInputField nameInputField;
        [SerializeField] internal LimitInputField descriptionInputField;

        [SerializeField] internal TMP_Dropdown landsDropDown;

        internal BuilderScene scene;

        internal Dictionary<string, LandWithAccess> landsDropdownDictionary = new Dictionary<string, LandWithAccess>();

        public override void RefreshControl()
        {
            if (scene == null)
                return;

            SetProjectToPublish(scene);
        }

        public override void Awake()
        {
            base.Awake();
            modal.OnCloseAction += CancelPublish;

            cancelButton.onClick.AddListener(CancelButtonPressed);
            publishButton.onClick.AddListener(PublishButtonPressed);
        }

        public override void Dispose()
        {
            base.Dispose();

            modal.OnCloseAction -= CancelPublish;

            cancelButton.onClick.RemoveAllListeners();
            publishButton.onClick.RemoveAllListeners();
        }

        public void SetProjectToPublish(BuilderScene scene)
        {
            this.scene = scene;

            //We set the screenshot
            sceneScreenshotImage.texture = scene.sceneScreenshotTexture;

            //We set the scene info
            nameInputField.SetText(scene.manifest.project.title);
            descriptionInputField.SetText(scene.manifest.project.description);
            sceneScreenshotParcelText.text = GetScreenshotText(scene.scene.sceneData.parcels);

            //We fill the land drop down
            FillLandDropDown();
        }

        private string GetScreenshotText(Vector2Int[] parcels)
        {
            Vector2Int sceneSize = BIWUtils.GetSceneSize(parcels);
            return string.Format(SCREENSHOT_TEXT, parcels.Length, sceneSize.x * DCL.Configuration.ParcelSettings.PARCEL_SIZE,  DCL.Configuration.ParcelSettings.PARCEL_SIZE * sceneSize.y);
        }

        private void FillLandDropDown()
        {
            landsDropdownDictionary.Clear();

            List<TMP_Dropdown.OptionData> landsOption =  new List<TMP_Dropdown.OptionData>();
            foreach (var land in DataStore.i.builderInWorld.landsWithAccess.Get())
            {
                TMP_Dropdown.OptionData landData = new TMP_Dropdown.OptionData();
                string text = land.name + " " + land.baseCoords.x + "," + land.baseCoords.y;
                landData.text = text;
                landsOption.Add(landData);
                landsDropdownDictionary.Add(text, land);
            }

            landsDropDown.options = landsOption;
        }

        public void Show() { modal.Show(); }

        public void Hide() { modal.Hide(); }

        private void PublishButtonPressed()
        {
            modal.Hide();
            OnPublishButtonPressed?.Invoke();
        }

        private void CancelPublish() { OnCancel?.Invoke(); }

        private void CancelButtonPressed()
        {
            modal.Hide();
            CancelPublish();
        }

    }
}