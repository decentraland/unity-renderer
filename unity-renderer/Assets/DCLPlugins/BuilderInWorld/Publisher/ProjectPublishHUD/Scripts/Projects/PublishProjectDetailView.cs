using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace DCL.Builder
{
    public interface IPublishProjectDetailView
    {
        /// <summary>
        /// If the publish is canceled this action will be called
        /// </summary>
        event Action OnCancel;

        /// <summary>
        /// If the publish button is pressed this action will be called
        /// </summary>
        event Action<PublishInfo> OnPublishButtonPressed;

        /// <summary>
        /// If the rotation of the project is changed, this event will be fired
        /// </summary>
        event Action<PublishInfo.ProjectRotation>  OnProjectRotateChange;

        /// <summary>
        /// Update the size of the project
        /// </summary>
        /// <param name="parcels"></param>
        void UpdateProjectSize(Vector2Int[] parcels);
        
        /// <summary>
        /// Set the project to publish
        /// </summary>
        /// <param name="scene"></param>
        void SetProjectToPublish(IBuilderScene scene);

        /// <summary>
        /// This will reset the view to the first state
        /// </summary>
        void ResetView();

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
        private const float SECONDS_TO_HIDE_TOP_TOAST = 4.5f;
        public event Action<PublishInfo.ProjectRotation> OnProjectRotateChange;
        public event Action OnCancel;
        public event Action<PublishInfo> OnPublishButtonPressed;

        [SerializeField] internal ModalComponentView modal;

        [Header("First step")]
        [SerializeField] internal GameObject firstStep;

        [SerializeField] internal Button backButton;
        [SerializeField] internal Button nextButton;
        [SerializeField] internal Button rotateLeftButton;
        [SerializeField] internal Button rotateRightButton;

        [SerializeField] internal GameObject aerialScreenshotGameObject;
        [SerializeField] internal RawImage sceneAerialScreenshotImage;

        [SerializeField] internal LimitInputField nameInputField;
        [SerializeField] internal LimitInputField descriptionInputField;

        [Header("Second step")]
        [SerializeField] internal GameObject secondStep;
        [SerializeField] internal Button backSecondButton;
        [SerializeField] internal Button publishButton;
        [SerializeField] internal PublishLandListView landListView;
        [SerializeField] internal ProjectPublishToast toastView;
        [SerializeField] internal PublishMapView mapView;
        [SerializeField] internal SearchLandView searchView;
        [SerializeField] internal Button miniSearchView;
        [SerializeField] internal TextMeshProUGUI topToastText;
        [SerializeField] internal GameObject topToastGameObject;
        
        [SerializeField] internal RawImage sceneScreenshotImage;

        internal IBuilderScene scene;
        private PublishInfo.ProjectRotation projectRotation = PublishInfo.ProjectRotation.NORTH;
        internal int currentStep = 0;
        internal List<Vector2Int> availableLandsToPublish = new List<Vector2Int>();

        internal Vector2Int selectedCoords;
        internal bool areCoordsSelected = false;
        private Coroutine toastTopHideCoroutine;
  

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

            landListView.OnLandSelected += LandSelected;
            mapView.OnParcelHover += ParcelHovered;
            mapView.OnParcelClicked += ParcelClicked;
            searchView.OnValueSearch += SearchLand;
            searchView.OnSearchCanceled += DeactivateSearch;

            backButton.onClick.AddListener(Back);
            nextButton.onClick.AddListener(Next);

            backSecondButton.onClick.AddListener(Back);
            publishButton.onClick.AddListener(PublishButtonPressed);

            rotateLeftButton.onClick.AddListener( RotateLeft);
            rotateRightButton.onClick.AddListener( RotateRight);
            miniSearchView.onClick.AddListener(ShowSearchBar);

            nameInputField.OnEmptyValue += DisableNextButton;
            nameInputField.OnLimitReached += DisableNextButton;
            nameInputField.OnInputAvailable += EnableNextButton;
            
            descriptionInputField.OnLimitReached += DisableNextButton;
            descriptionInputField.OnInputAvailable += EnableNextButton;

            gameObject.SetActive(false);
            searchView.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            base.Dispose();

            modal.OnCloseAction -= CancelPublish;

            mapView.OnParcelHover -= ParcelHovered;
            mapView.OnParcelClicked -= ParcelClicked;
            landListView.OnLandSelected -= LandSelected;
            searchView.OnValueSearch -= SearchLand;
            searchView.OnSearchCanceled -= DeactivateSearch;

            backButton.onClick.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();

            backSecondButton.onClick.RemoveAllListeners();
            publishButton.onClick.RemoveAllListeners();

            rotateLeftButton.onClick.RemoveAllListeners();
            rotateRightButton.onClick.RemoveAllListeners();
            miniSearchView.onClick.RemoveAllListeners();
            
            nameInputField.OnLimitReached -= DisableNextButton;
            nameInputField.OnInputAvailable -= EnableNextButton;
            nameInputField.OnEmptyValue -= DisableNextButton;
            
            descriptionInputField.OnLimitReached -= DisableNextButton;
            descriptionInputField.OnInputAvailable -= EnableNextButton;
        }

        private void DeactivateSearch()
        {
            landListView.SetActive(false);
        }

        private void Back()
        {
            if (currentStep <= 0)
            {
                Hide();
                return;
            }

            currentStep--;
            ShowCurrentStep();
        }

        private void Next()
        {
            currentStep++;
            ShowCurrentStep();
        }

        private void SearchLand(string searchInput)
        {
            List<LandWithAccess> filteredLands = new List<LandWithAccess>();
            foreach (LandWithAccess land in DataStore.i.builderInWorld.landsWithAccess.Get())
            {
                bool shouldBeAdded = land.name.ToLower().Contains(searchInput.ToLower());

                foreach (Vector2Int landParcel in land.parcels)
                {
                    if(BIWUtils.Vector2INTToString(landParcel).Contains(searchInput))
                        shouldBeAdded = true;
                }
                
                if(shouldBeAdded && !filteredLands.Contains(land))
                    filteredLands.Add(land);
            }

            // We fill the list with the lands
            landListView.SetContent(scene.manifest.project.cols, scene.manifest.project.rows, filteredLands);
            landListView.SetActive(true);
        }

        private void ShowTopToast(string text)
        {
            topToastText.text = text;
            topToastGameObject.SetActive(true);
            HideSearchBar();

            toastTopHideCoroutine = StartCoroutine(WaitAndHideTopToast());
        }
        
        private void ShowSearchBar()
        {
            searchView.gameObject.SetActive(true);
            miniSearchView.gameObject.SetActive(false);
            HideTopToast();
        }
        
        private void HideSearchBar()
        {
            searchView.gameObject.SetActive(false);
            miniSearchView.gameObject.SetActive(true);
            landListView.HideEmptyContent();
            landListView.SetActive(false);
        }

        private void HideTopToast()
        {
            if (toastTopHideCoroutine != null)
                StopCoroutine(toastTopHideCoroutine);
            topToastGameObject.SetActive(false);
        }
        
        private void ParcelClicked(Vector2Int parcel)
        {
            if (!availableLandsToPublish.Contains(parcel))
            {
                ShowTopToast("Projects can't be placed in a Land you don't own.");
                return;
            }

            LandSelected(parcel);
        }

        public void LandSelected(Vector2Int parcel)
        {
            foreach (LandWithAccess land in DataStore.i.builderInWorld.landsWithAccess.Get())
            {
                foreach (Vector2Int landParcel in land.parcels)
                {
                    if (landParcel == parcel)
                    {
                        toastView.SetLandInfo(land.name, BIWUtils.Vector2INTToString(landParcel));
                        bool isEmpty = !(land.scenes.Count == 0 || land.scenes[0].isEmpty);
                        toastView.SetSubtitleActive(isEmpty);
                        toastView.Show();
                        break;
                    }
                }

            }

            mapView.SelectLandInMap(parcel);
            areCoordsSelected = true;
            selectedCoords = parcel;
            publishButton.interactable = true;
        }

        private void ParcelHovered(Vector2Int parcel)
        {
            bool isAvailable = availableLandsToPublish.Contains(parcel);
            mapView.SetAvailabilityToPublish(isAvailable);
        }

        private void ShowCurrentStep()
        {
            firstStep.SetActive(false);
            secondStep.SetActive(false);
            switch (currentStep)
            {
                case 0: // Choose name, desc and rotation
                    firstStep.SetActive(true);
                    break;
                case 1: // Choose land to deploy
                    searchView.ClearSearch();
                    HideSearchBar();
                    secondStep.SetActive(true);
                    if (availableLandsToPublish.Count > 0)
                        GoToCoords(availableLandsToPublish[0]);

                    if (areCoordsSelected)
                    {
                        LandSelected(selectedCoords);
                        GoToCoords(selectedCoords);
                        toastView.Show(true);
                    }
                    break;
            }
        }

        private void RotateLeft()
        {
            projectRotation--;
            if (projectRotation < 0)
                projectRotation = PublishInfo.ProjectRotation.WEST;
            SetRotation(projectRotation);
        }

        private void RotateRight()
        {
            projectRotation++;
            if (projectRotation > PublishInfo.ProjectRotation.WEST)
                projectRotation = PublishInfo.ProjectRotation.NORTH;
            SetRotation(projectRotation);
        }

        private void SetRotation(PublishInfo.ProjectRotation rotation)
        {
            float zRotation = 0;
            switch (rotation)
            {
                case PublishInfo.ProjectRotation.NORTH:
                    zRotation = 0;
                    break;
                case PublishInfo.ProjectRotation.EAST:
                    zRotation = 270;
                    break;
                case PublishInfo.ProjectRotation.SOUTH:
                    zRotation = 180;
                    break;
                case PublishInfo.ProjectRotation.WEST:
                    zRotation = 90;
                    break;
            }
            sceneAerialScreenshotImage.rectTransform.rotation = Quaternion.Euler(0, 0, zRotation);
            OnProjectRotateChange?.Invoke(rotation);
        }

        public void SetProjectToPublish(IBuilderScene scene)
        {
            this.scene = scene;

            // Reset common views
            toastView.Hide(true);
            HideSearchBar();
            
            // We reset the selected coords and disable the publish button until the coords are selected
            areCoordsSelected = false;
            publishButton.interactable = false;

            // We set the screenshot
            if (scene.aerialScreenshotTexture != null && BIWUtils.IsParcelSceneSquare(scene.scene.sceneData.parcels))
            {
                aerialScreenshotGameObject.SetActive(true);
                sceneAerialScreenshotImage.texture = scene.aerialScreenshotTexture;
            }
            else
            {
                aerialScreenshotGameObject.SetActive(false);
            }

            if (sceneScreenshotImage != null)
                sceneScreenshotImage.texture = scene.sceneScreenshotTexture;

            // We set the scene info
            nameInputField.SetText(scene.manifest.project.title);
            descriptionInputField.SetText(scene.manifest.project.description);

            // We filter the available lands
            CheckAvailableLandsToPublish(scene);

            // We set the size of the project in the builder
            UpdateProjectSize(scene.scene.sceneData.parcels);
        }

        public void UpdateProjectSize(Vector2Int[] parcels)
        {
            // We set the size of the project in the builder
            mapView.SetProjectSize(parcels);
        }

        private void CheckAvailableLandsToPublish(IBuilderScene sceneToPublish)
        {
            availableLandsToPublish.Clear();
            availableLandsToPublish = BIWUtils.GetLandsToPublishProject(DataStore.i.builderInWorld.landsWithAccess.Get(), sceneToPublish);
        }

        private void LandSelected(LandWithAccess land)
        {
            landListView.SetActive(false);
            searchView.ClearSearch();
            HideSearchBar();
            
            // We select the land
            LandSelected(land.baseCoords);
            
            // We set the map to the main land
            GoToCoords(land.baseCoords);
        }

        private void GoToCoords(Vector2Int coord)
        {
            // We set the map to the main land
            CoroutineStarter.Start(WaitFrameToPositionMap(coord));
        }

        public void Show()
        {
            gameObject.SetActive(true);
            mapView.SetVisible(true);
            modal.Show();
            if (areCoordsSelected)
            {
                LandSelected(selectedCoords);
                GoToCoords(selectedCoords);
                toastView.Show(true);
            }
        }

        public void ResetView()
        {
            currentStep = 0;
            ShowCurrentStep();
        }

        public void Hide()
        {
            if(!modal.isVisible)
                return;
            
            modal.Hide();
            mapView.SetVisible(false);
            CancelPublish();
        }

        internal void EnableNextButton()
        {
            if(nameInputField.IsInputAvailable() && descriptionInputField.IsInputAvailable())
                nextButton.interactable = true;
        }

        internal void DisableNextButton() { nextButton.interactable = false; }

        private void PublishButtonPressed()
        {
            Hide();
            PublishInfo publishInfo = new PublishInfo();
            scene.manifest.project.title = nameInputField.GetValue();
            scene.manifest.project.description = descriptionInputField.GetValue();
            publishInfo.coordsToPublish = selectedCoords;

            OnPublishButtonPressed?.Invoke(publishInfo);
        }

        private void CancelPublish() { OnCancel?.Invoke(); }

        private void CancelButtonPressed()
        {
            Hide();
            CancelPublish();
        }
        
        IEnumerator WaitFrameToPositionMap(Vector2Int coords)
        {
            yield return null;
            mapView.GoToCoords(coords);
        }

        IEnumerator WaitAndHideTopToast()
        {
            yield return new WaitForSeconds(SECONDS_TO_HIDE_TOP_TOAST);
            HideTopToast();
        }
    }
}