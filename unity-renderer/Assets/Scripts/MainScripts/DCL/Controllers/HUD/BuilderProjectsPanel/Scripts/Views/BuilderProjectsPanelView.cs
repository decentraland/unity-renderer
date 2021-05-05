using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

internal class BuilderProjectsPanelView : MonoBehaviour, IDeployedSceneListener, IProjectSceneListener
{
    [Header("References")]
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button createSceneButton;
    [SerializeField] internal Button importSceneButton;

    [SerializeField] internal Transform sectionsContainer;

    [SerializeField] internal LeftMenuButtonToggleView scenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView inWorldScenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView projectsToggle;
    [SerializeField] internal LeftMenuButtonToggleView landToggle;

    [SerializeField] internal SceneCardViewContextMenu contextMenu;

    [Header("Prefabs")]
    [SerializeField] internal SceneCardView sceneCardViewPrefab;

    public event Action OnClosePressed;
    public event Action OnCreateScenePressed;
    public event Action OnImportScenePressed;
    public event Action<bool> OnScenesToggleChanged;
    public event Action<bool> OnInWorldScenesToggleChanged;
    public event Action<bool> OnProjectsToggleChanged;
    public event Action<bool> OnLandToggleChanged;

    private int deployedScenesCount = 0;
    private int projectScenesCount = 0;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
        createSceneButton.onClick.AddListener(() => OnCreateScenePressed?.Invoke());
        importSceneButton.onClick.AddListener(() => OnImportScenePressed?.Invoke());

        scenesToggle.OnToggleValueChanged += OnScenesToggleChanged;
        inWorldScenesToggle.OnToggleValueChanged += OnInWorldScenesToggleChanged;
        projectsToggle.OnToggleValueChanged += OnProjectsToggleChanged;
        landToggle.OnToggleValueChanged += OnLandToggleChanged;

        contextMenu.Hide();
    }

    private void SubmenuScenesDirty()
    {
        inWorldScenesToggle.gameObject.SetActive(deployedScenesCount > 0);
        projectsToggle.gameObject.SetActive(projectScenesCount > 0);
    }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, ISceneCardView> scenes)
    {
        deployedScenesCount = scenes.Count;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, ISceneCardView> scenes)
    {
        projectScenesCount = scenes.Count;
        SubmenuScenesDirty();
    }

    void IDeployedSceneListener.OnSceneAdded(ISceneCardView scene)
    {
        deployedScenesCount++;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSceneAdded(ISceneCardView scene)
    {
        projectScenesCount++;
        SubmenuScenesDirty();
    }

    void IDeployedSceneListener.OnSceneRemoved(ISceneCardView scene)
    {
        deployedScenesCount--;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSceneRemoved(ISceneCardView scene)
    {
        projectScenesCount--;
        SubmenuScenesDirty();
    }
}