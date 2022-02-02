using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IExperiencesViewerComponentView
{
    /// <summary>
    /// It will be triggered when the close button is clicked.
    /// </summary>
    event Action onCloseButtonPressed;

    /// <summary>
    /// It will be triggered when the UI visibility of some experience changes.
    /// </summary>
    event Action<string, bool> onSomeExperienceUIVisibilityChanged;

    /// <summary>
    /// It will be triggered when the execution of some experience changes.
    /// </summary>
    event Action<string, bool> onSomeExperienceExecutionChanged;

    /// <summary>
    /// Set the available experiences component with a list of experiences.
    /// </summary>
    /// <param name="experiences">List of experiences (model) to be loaded.</param>
    void SetAvailableExperiences(List<ExperienceRowComponentModel> experiences);

    /// <summary>
    /// Add an experience into the available experiences component.
    /// </summary>
    /// <param name="experience">Experience to add.</param>
    void AddAvailableExperience(ExperienceRowComponentModel experience);

    /// <summary>
    /// Remove an experience from the available experiences component.
    /// </summary>
    /// <param name="id">Experience id to remove.</param>
    void RemoveAvailableExperience(string id);

    /// <summary>
    /// Shows/Hides the game object of the Experiences Viewer.
    /// </summary>
    /// <param name="isActive">True to show it.</param>
    void SetVisible(bool isActive);
}

public class ExperiencesViewerComponentView : BaseComponentView, IExperiencesViewerComponentView
{
    internal const string EXPERIENCES_POOL_NAME = "ExperiencesViewer_ExperienceRowsPool";
    internal const int EXPERIENCES_POOL_PREWARM = 10;

    [Header("Assets References")]
    [SerializeField] internal ExperienceRowComponentView experienceRowPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ButtonComponentView closeButton;
    [SerializeField] internal GridContainerComponentView availableExperiences;
    public Color rowsBackgroundColor;
    public Color rowsOnHoverColor;

    public event Action onCloseButtonPressed;
    public event Action<string, bool> onSomeExperienceUIVisibilityChanged;
    public event Action<string, bool> onSomeExperienceExecutionChanged;

    internal Pool experiencesPool;

    public override void Awake()
    {
        base.Awake();

        closeButton.onClick.AddListener(() => onCloseButtonPressed?.Invoke());

        ConfigurePEXPool();
    }

    public override void RefreshControl()
    {
        availableExperiences.RefreshControl();
    }

    public void SetAvailableExperiences(List<ExperienceRowComponentModel> experiences)
    {
        availableExperiences.ExtractItems();
        experiencesPool.ReleaseAll();

        List<BaseComponentView> experiencesToAdd = new List<BaseComponentView>();
        foreach (ExperienceRowComponentModel exp in experiences)
        {
            AddAvailableExperience(exp);
        }
    }

    public void AddAvailableExperience(ExperienceRowComponentModel experience)
    {
        experience.backgroundColor = rowsBackgroundColor;
        experience.onHoverColor = rowsOnHoverColor;

        ExperienceRowComponentView newRealmRow = experiencesPool.Get().gameObject.GetComponent<ExperienceRowComponentView>();
        newRealmRow.Configure(experience);
        newRealmRow.onShowPEXUI -= OnSomeExperienceUIVisibilityChanged;
        newRealmRow.onShowPEXUI += OnSomeExperienceUIVisibilityChanged;
        newRealmRow.onStartPEX -= OnSomeExperienceExecutionChanged;
        newRealmRow.onStartPEX += OnSomeExperienceExecutionChanged;

        availableExperiences.AddItem(newRealmRow);
    }

    public void RemoveAvailableExperience(string id)
    {
        ExperienceRowComponentView experienceToRemove = availableExperiences.GetItems()
            .Select(x => x as ExperienceRowComponentView)
            .FirstOrDefault(x => x.model.id == id);

        if (experienceToRemove != null)
            availableExperiences.RemoveItem(experienceToRemove);
    }

    public void SetVisible(bool isActive) { gameObject.SetActive(isActive); }

    public override void Dispose()
    {
        base.Dispose();

        closeButton.onClick.RemoveAllListeners();
    }

    internal void OnSomeExperienceUIVisibilityChanged(string pexId, bool isUIVisible)
    {
        onSomeExperienceUIVisibilityChanged?.Invoke(pexId, isUIVisible);
    }

    internal void OnSomeExperienceExecutionChanged(string pexId, bool isPlaying)
    {
        onSomeExperienceExecutionChanged?.Invoke(pexId, isPlaying);
    }

    internal void ConfigurePEXPool()
    {
        experiencesPool = PoolManager.i.GetPool(EXPERIENCES_POOL_NAME);
        if (experiencesPool == null)
        {
            experiencesPool = PoolManager.i.AddPool(
                EXPERIENCES_POOL_NAME,
                GameObject.Instantiate(experienceRowPrefab).gameObject,
                maxPrewarmCount: EXPERIENCES_POOL_PREWARM,
                isPersistent: true);

            experiencesPool.ForcePrewarm();
        }
    }

    internal static ExperiencesViewerComponentView Create()
    {
        ExperiencesViewerComponentView experiencesViewerComponentView = Instantiate(Resources.Load<GameObject>("ExperiencesViewer")).GetComponent<ExperiencesViewerComponentView>();
        experiencesViewerComponentView.name = "_ExperiencesViewer";

        return experiencesViewerComponentView;
    }
}
