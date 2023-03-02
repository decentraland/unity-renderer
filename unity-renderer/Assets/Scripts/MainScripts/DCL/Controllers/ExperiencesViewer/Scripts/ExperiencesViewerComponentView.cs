using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.ExperiencesViewer
{
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
        /// It represents the container transform of the component.
        /// </summary>
        Transform experienceViewerTransform { get; }

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
        /// Get all experiences.
        /// </summary>
        /// <returns>A list of experiences.</returns>
        List<ExperienceRowComponentView> GetAllAvailableExperiences();

        /// <summary>
        /// Get a specific experience.
        /// </summary>
        /// <param name="id">Id of the experience to search.</param>
        /// <returns>An experience.</returns>
        ExperienceRowComponentView GetAvailableExperienceById(string id);

        /// <summary>
        /// Shows/Hides the game object of the Experiences Viewer.
        /// </summary>
        /// <param name="isActive">True to show it.</param>
        void SetVisible(bool isActive);

        /// <summary>
        /// Shows the info toast for when the UI of an experience is hidden.
        /// </summary>
        void ShowUIHiddenToast();
    }

    public class ExperiencesViewerComponentView : BaseComponentView, IExperiencesViewerComponentView
    {
        internal const string EXPERIENCES_POOL_NAME = "ExperiencesViewer_ExperienceRowsPool";
        internal const int EXPERIENCES_POOL_PREWARM = 10;
        internal const float UI_HIDDEN_TOAST_SHOWING_TIME = 2f;

        [Header("Assets References")]
        [SerializeField] internal ExperienceRowComponentView experienceRowPrefab;

        [Header("Prefab References")]
        [SerializeField] internal ButtonComponentView closeButton;
        [SerializeField] internal GridContainerComponentView availableExperiences;
        [SerializeField] internal ShowHideAnimator hiddenUIToastAnimator;
        public Color rowsBackgroundColor;
        public Color rowsOnHoverColor;

        public event Action onCloseButtonPressed;
        public event Action<string, bool> onSomeExperienceUIVisibilityChanged;
        public event Action<string, bool> onSomeExperienceExecutionChanged;

        internal Pool experiencesPool;
        internal Coroutine uiHiddenToastCoroutine;

        public Transform experienceViewerTransform => transform;

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

            availableExperiences.AddItemWithResize(newRealmRow);
        }

        public void RemoveAvailableExperience(string id)
        {
            ExperienceRowComponentView experienceToRemove = availableExperiences.GetItems()
                .Select(x => x as ExperienceRowComponentView)
                .FirstOrDefault(x => x.model.id == id);

            if (experienceToRemove != null)
                availableExperiences.RemoveItem(experienceToRemove);
        }

        public List<ExperienceRowComponentView> GetAllAvailableExperiences()
        {
            return availableExperiences.GetItems()
                .Select(x => x as ExperienceRowComponentView)
                .ToList();
        }

        public ExperienceRowComponentView GetAvailableExperienceById(string id)
        {
            return availableExperiences.GetItems()
                .Select(x => x as ExperienceRowComponentView)
                .FirstOrDefault(x => x.model.id == id);
        }

        public void SetVisible(bool isActive) { gameObject.SetActive(isActive); }

        public void ShowUIHiddenToast()
        {
            if (uiHiddenToastCoroutine != null)
            {
                StopCoroutine(uiHiddenToastCoroutine);
                uiHiddenToastCoroutine = null;
            }

            uiHiddenToastCoroutine = StartCoroutine(ShowUIHiddenToastCoroutine());
        }

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

        internal IEnumerator ShowUIHiddenToastCoroutine()
        {
            hiddenUIToastAnimator.Show();
            yield return new WaitForSeconds(UI_HIDDEN_TOAST_SHOWING_TIME);
            hiddenUIToastAnimator.Hide();
        }

        internal static ExperiencesViewerComponentView Create()
        {
            ExperiencesViewerComponentView experiencesViewerComponentView = Instantiate(Resources.Load<GameObject>("ExperiencesViewer")).GetComponent<ExperiencesViewerComponentView>();
            experiencesViewerComponentView.name = "_ExperiencesViewer";

            return experiencesViewerComponentView;
        }
    }
}