using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    internal interface IBuilderMainPanelView : IDisposable
    {
        event Action OnBackToMainMenuPressed;
        event Action OnClosePressed;
        event Action OnBackPressed;
        event Action OnCreateProjectPressed;
        void SetVisible(bool visible);
        bool IsVisible();
        void SetTogglOnWithoutNotify(SectionId sectionId);
        void SetMainLeftPanel();
        void SetProjectSettingsLeftPanel();
        SceneCardView GetSceneCardViewPrefab();
        ProjectCardView GetProjectCardView();
        Transform GetSectionContainer();
        Transform GetTransform();
        SearchBarView GetSearchBar();
        LeftMenuSettingsViewReferences GetSettingsViewReferences();
        SceneCardViewContextMenu GetSceneCardViewContextMenu();
        IUnpublishPopupView GetUnpublishPopup();
    }

    internal class BuilderMainPanelView : MonoBehaviour, IBuilderMainPanelView, ISceneListener, IProjectsListener
    {
        [Header("General")]
        [SerializeField] internal Button closeButton;
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal Transform sectionsContainer;
        [SerializeField] internal SceneCardViewContextMenu contextMenu;
        [SerializeField] internal SearchBarView searchBarView;
        [SerializeField] internal ShowHideAnimator showHideAnimator;
        [SerializeField] internal InputAction_Trigger closeTrigger;

        [Header("Left-Panel Section Buttons")]
        [SerializeField] internal LeftMenuButtonToggleView[] sectionToggles;

        [Header("Left-Panel")]
        [SerializeField] internal GameObject leftPanelMain;
        [SerializeField] internal GameObject leftPanelProjectSettings;
        [SerializeField] internal Button createSceneButton;
        [SerializeField] internal Button importSceneButton;
        [SerializeField] internal Button backToMainPanelButton;
        [SerializeField] internal LeftMenuSettingsViewReferences settingsViewReferences;

        [Header("Assets")]
        [SerializeField] internal SceneCardView sceneCardViewPrefab;
        [SerializeField] internal ProjectCardView projectCardView;

        [Header("Popups")]
        [SerializeField] internal UnpublishPopupView unpublishPopupView;

        public event Action OnClosePressed;
        public event Action OnBackPressed;
        public event Action OnCreateProjectPressed;
        public event Action OnImportScenePressed;
        public event Action OnBackToMainMenuPressed;

        private int scenesCount = 0;
        private int projectScenesCount = 0;
        private bool isDestroyed = false;

        public bool IsVisible() { return gameObject.activeSelf; }

        void IBuilderMainPanelView.SetVisible(bool visible)
        {
            if (visible)
            {
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }
                showHideAnimator.Show();
            }
            else
            {
                showHideAnimator.Hide();
            }
        }

        void IBuilderMainPanelView.SetTogglOnWithoutNotify(SectionId sectionId)
        {
            for (int i = 0; i < sectionToggles.Length; i++)
            {
                sectionToggles[i].SetIsOnWithoutNotify(sectionId == sectionToggles[i].openSection);
            }
        }

        void IBuilderMainPanelView.SetMainLeftPanel()
        {
            leftPanelMain.SetActive(true);
            leftPanelProjectSettings.SetActive(false);
        }

        void IBuilderMainPanelView.SetProjectSettingsLeftPanel()
        {
            leftPanelMain.SetActive(false);
            leftPanelProjectSettings.SetActive(true);
        }

        ProjectCardView IBuilderMainPanelView.GetProjectCardView() { return projectCardView; }
        
        SceneCardView IBuilderMainPanelView.GetSceneCardViewPrefab() { return sceneCardViewPrefab; }

        Transform IBuilderMainPanelView.GetSectionContainer() { return sectionsContainer; }

        Transform IBuilderMainPanelView.GetTransform() { return transform; }

        SearchBarView IBuilderMainPanelView.GetSearchBar() { return  searchBarView; }

        LeftMenuSettingsViewReferences IBuilderMainPanelView.GetSettingsViewReferences() { return settingsViewReferences; }

        SceneCardViewContextMenu IBuilderMainPanelView.GetSceneCardViewContextMenu() { return contextMenu; }

        IUnpublishPopupView IBuilderMainPanelView.GetUnpublishPopup() { return unpublishPopupView; }

        public void Dispose()
        {
            if (!isDestroyed)
            {
                Destroy(gameObject);
            }
        }

        private void Awake()
        {
            name = "_BuilderProjectsPanel";

            closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
            backgroundButton?.onClick.AddListener(() => OnClosePressed?.Invoke());
            createSceneButton.onClick.AddListener(() => OnCreateProjectPressed?.Invoke());
            importSceneButton.onClick.AddListener(() => OnImportScenePressed?.Invoke());
            backToMainPanelButton.onClick.AddListener(() => OnBackToMainMenuPressed?.Invoke());
            closeTrigger.OnTriggered += CloseTriggerOnOnTriggered;

            contextMenu.Hide();
            gameObject.SetActive(false);

            for (int i = 0; i < sectionToggles.Length; i++)
            {
                sectionToggles[i].Setup();
            }
        }

        private void OnDestroy() { isDestroyed = true; }

        private void CloseTriggerOnOnTriggered(DCLAction_Trigger action) { OnBackPressed?.Invoke(); }

        void ISceneListener.SetScenes(Dictionary<string, ISceneCardView> scenes) { scenesCount = scenes.Count; }

        void ISceneListener.SceneAdded(ISceneCardView scene) { scenesCount++; }

        void ISceneListener.SceneRemoved(ISceneCardView scene) { scenesCount--; }

        public void OnSetProjects(Dictionary<string, IProjectCardView> projects) { projectScenesCount = projects.Count; }
        
        public void OnProjectAdded(IProjectCardView project) { projectScenesCount++; }
        
        public void OnProjectRemoved(IProjectCardView project) { projectScenesCount--; }
    }
}