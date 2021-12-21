using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
        public event Action<PublishInfo> OnPublishButtonPressed;

        /// <summary>
        /// If the rotate left button is pressed this action will be called
        /// </summary>
        public event Action OnProjectRotateLeft;

        /// <summary>
        /// If the rotate right button is pressed this action will be called
        /// </summary>
        public event Action OnProjectRotateRight;

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

        public event Action OnProjectRotateLeft;
        public event Action OnProjectRotateRight;
        public event Action OnCancel;
        public event Action<PublishInfo> OnPublishButtonPressed;

        [Header("Map Renderer")]
        public PublishMapView mapView;

        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button publishButton;

        //TODO: This functionality will be implemented in the future
        [SerializeField] internal Button rotateLeftButton;
        [SerializeField] internal Button rotateRightButton;

        [SerializeField] internal Button zoomInButton;
        [SerializeField] internal Button zoomOutButton;
        [SerializeField] internal Button resetToLandButton;

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

            rotateLeftButton.onClick.AddListener( (() => OnProjectRotateLeft?.Invoke()));
            rotateRightButton.onClick.AddListener( (() => OnProjectRotateRight?.Invoke()));

            landsDropDown.onValueChanged.AddListener(LandSelectedFromDropDown);

            mapView.OnParcelClicked += LandSelectedFromMap;

            gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            base.Dispose();

            modal.OnCloseAction -= CancelPublish;
            mapView.OnParcelClicked -= LandSelectedFromMap;

            cancelButton.onClick.RemoveAllListeners();
            publishButton.onClick.RemoveAllListeners();

            landsDropDown.onValueChanged.RemoveAllListeners();

            rotateLeftButton.onClick.RemoveAllListeners();
            rotateRightButton.onClick.RemoveAllListeners();
        }

        private void LandSelectedFromDropDown(int index)
        {
            //We set the map to the main land
            Vector2Int coordsToHighlight = landsDropdownDictionary[landsDropDown.options[index].text].baseCoords;
            mapView.GoToCoords(coordsToHighlight);
        }

        private void LandSelectedFromMap(Vector2Int coords)
        {
            foreach (var land in DataStore.i.builderInWorld.landsWithAccess.Get())
            {
                foreach (Vector2Int landParcel in land.parcels)
                {
                    if (landParcel == coords)
                    {
                        string text = GetLandText(land.name, landParcel);
                        for (int i = 0 ; i < landsDropDown.options.Count; i++)
                        {
                            if (text == landsDropDown.options[i].text)
                                landsDropDown.SetValueWithoutNotify(i);
                        }
                    }
                }
            }
            mapView.GoToCoords(coords);
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

            //We set the map to the main land
            Vector2Int coordsToHighlight = landsDropdownDictionary[landsDropDown.options[landsDropDown.value].text].baseCoords;
            mapView.GoToCoords(coordsToHighlight);
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
                foreach (Vector2Int landParcel in land.parcels)
                {
                    string text = GetLandText(land.name, landParcel);

                    if (landsDropdownDictionary.ContainsKey(text))
                        continue;

                    TMP_Dropdown.OptionData landData = new TMP_Dropdown.OptionData();
                    landData.text = text;
                    landsOption.Add(landData);
                    landsDropdownDictionary.Add(text, land);
                }
            }

            landsDropDown.options = landsOption;
        }

        internal string GetLandText(string landName, Vector2Int landParcel) { return landName + " (" + landParcel.x + "," + landParcel.y + ")"; }

        public void Show()
        {
            gameObject.SetActive(true);
            modal.Show();
            mapView.SetVisible(true);
        }

        public void Hide()
        {
            modal.Hide();
            mapView.SetVisible(false);
        }

        private void PublishButtonPressed()
        {
            Hide();
            PublishInfo publishInfo = new PublishInfo();
            publishInfo.name = nameInputField.GetValue();
            publishInfo.description = descriptionInputField.GetValue();
            publishInfo.landToPublish = landsDropdownDictionary[landsDropDown.options[landsDropDown.value].text];

            OnPublishButtonPressed?.Invoke(publishInfo);
        }

        private void CancelPublish() { OnCancel?.Invoke(); }

        private void CancelButtonPressed()
        {
            Hide();
            CancelPublish();
        }

    }
}