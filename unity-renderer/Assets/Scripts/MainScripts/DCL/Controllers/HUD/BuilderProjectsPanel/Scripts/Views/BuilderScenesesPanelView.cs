using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal interface IBuilderProjectsPanelView : IDisposable
{
    event Action OnBackToMainMenuPressed;
    event Action OnClosePressed;
    void SetVisible(bool visible);
    bool IsVisible();
    void SetTogglOnWithoutNotify(SectionId sectionId);
    void SetMainLeftPanel();
    void SetProjectSettingsLeftPanel();
    SceneCardView GetCardViewPrefab();
    Transform GetSectionContainer();
    Transform GetTransform();
    SearchBarView GetSearchBar();
    LeftMenuSettingsViewReferences GetSettingsViewReferences();
    SceneCardViewContextMenu GetSceneCardViewContextMenu();
    IUnpublishPopupView GetUnpublishPopup();
}

internal class BuilderScenesesPanelView : MonoBehaviour, IBuilderProjectsPanelView, IDeployedSceneListener, IScenesListener
{
    [Header("General")]
    [SerializeField] internal Button closeButton;
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

    [Header("Popups")]
    [SerializeField] internal UnpublishPopupView unpublishPopupView;

    public event Action OnClosePressed;
    public event Action OnCreateScenePressed;
    public event Action OnImportScenePressed;
    public event Action OnBackToMainMenuPressed;

    private int scenesCount = 0;
    private int projectScenesCount = 0;
    private bool isDestroyed = false;

    public bool IsVisible() { return gameObject.activeSelf; }

    void IBuilderProjectsPanelView.SetVisible(bool visible)
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

    void IBuilderProjectsPanelView.SetTogglOnWithoutNotify(SectionId sectionId)
    {
        for (int i = 0; i < sectionToggles.Length; i++)
        {
            sectionToggles[i].SetIsOnWithoutNotify(sectionId == sectionToggles[i].openSection);
        }
    }

    void IBuilderProjectsPanelView.SetMainLeftPanel()
    {
        leftPanelMain.SetActive(true);
        leftPanelProjectSettings.SetActive(false);
    }

    void IBuilderProjectsPanelView.SetProjectSettingsLeftPanel()
    {
        leftPanelMain.SetActive(false);
        leftPanelProjectSettings.SetActive(true);
    }

    SceneCardView IBuilderProjectsPanelView.GetCardViewPrefab() { return sceneCardViewPrefab; }

    Transform IBuilderProjectsPanelView.GetSectionContainer() { return sectionsContainer; }

    Transform IBuilderProjectsPanelView.GetTransform() { return transform; }

    SearchBarView IBuilderProjectsPanelView.GetSearchBar() { return  searchBarView; }

    LeftMenuSettingsViewReferences IBuilderProjectsPanelView.GetSettingsViewReferences() { return settingsViewReferences; }

    SceneCardViewContextMenu IBuilderProjectsPanelView.GetSceneCardViewContextMenu() { return contextMenu; }

    IUnpublishPopupView IBuilderProjectsPanelView.GetUnpublishPopup() { return unpublishPopupView; }

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
        createSceneButton.onClick.AddListener(() => OnCreateScenePressed?.Invoke());
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

    private void CloseTriggerOnOnTriggered(DCLAction_Trigger action) { OnClosePressed?.Invoke(); }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, ISceneCardView> scenes) { scenesCount = scenes.Count; }

    void IScenesListener.OnSetScenes(Dictionary<string, ISceneCardView> scenes) { projectScenesCount = scenes.Count; }

    void IDeployedSceneListener.OnSceneAdded(ISceneCardView scene) { scenesCount++; }

    void IScenesListener.OnSceneAdded(ISceneCardView scene) { projectScenesCount++; }

    void IDeployedSceneListener.OnSceneRemoved(ISceneCardView scene) { scenesCount--; }

    void IScenesListener.OnSceneRemoved(ISceneCardView scene) { projectScenesCount--; }
}