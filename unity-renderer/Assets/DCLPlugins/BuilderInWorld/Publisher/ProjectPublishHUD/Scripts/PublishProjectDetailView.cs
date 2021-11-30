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
        public event Action OnCancel;
        public event Action OnPublish;

        /// <summary>
        /// Set the project to publish
        /// </summary>
        /// <param name="scene"></param>
        void SetBuilderScene(BuilderScene scene);

        void Hide();

        void Dispose();
    }

    public class PublishProjectDetailView : BaseComponentView, IPublishProjectDetailView
    {
        public event Action OnProjectRotate;
        public event Action OnCancel;
        public event Action OnPublish;

        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button publishButton;

        [SerializeField] internal Button rotateLeftButton;
        [SerializeField] internal Button rotateRightButton;

        [SerializeField] internal Button zoomInButton;
        [SerializeField] internal Button zoomOutButton;
        [SerializeField] internal Button resetToLandButton;

        [SerializeField] internal RawImage sceneScreenshotImage;
        [SerializeField] internal Image mapImage;

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

            SetBuilderScene(scene);
        }

        public override void Awake()
        {
            base.Awake();
            cancelButton.onClick.AddListener(Cancel);
            publishButton.onClick.AddListener(Publish);
        }

        public override void Dispose()
        {
            base.Dispose();
            cancelButton.onClick.RemoveAllListeners();
            publishButton.onClick.RemoveAllListeners();
        }

        public void SetBuilderScene(BuilderScene scene)
        {
            this.scene = scene;
            modal.Show();
            //We set the screenshot
            sceneScreenshotImage.texture = scene.sceneScreenshotTexture;

            //We set the scene info
            nameInputField.SetText(scene.manifest.project.title);
            descriptionInputField.SetText(scene.manifest.project.description);
            sceneScreenshotParcelText.text = GetScreenshotText(scene.scene.sceneData.parcels);
            FillLandDropDown();

        }

        private string GetScreenshotText(Vector2Int[] parcels)
        {
            Vector2Int sceneSize = BIWUtils.GetSceneSize(parcels);
            return parcels.Length + " parcel = " + sceneSize.x * DCL.Configuration.ParcelSettings.PARCEL_SIZE + "x" + DCL.Configuration.ParcelSettings.PARCEL_SIZE * sceneSize.y;
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

        public void Hide() { modal.Hide(); }

        private void Publish()
        {
            modal.Hide();
            OnPublish?.Invoke();
        }

        private void Cancel()
        {
            modal.Hide();
            OnCancel?.Invoke();
        }

    }
}