using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ExperiencesViewer
{
    public class ExperiencesViewerComponentView : BaseComponentView, IExperiencesViewerComponentView
    {
        private const int EXPERIENCES_POOL_PREWARM = 10;
        private const float UI_HIDDEN_TOAST_SHOWING_TIME = 2f;
        private const string EXPERIENCES_POOL_NAME = "ExperiencesViewer_ExperienceRowsPool";

        [Header("Assets References")]
        [SerializeField] internal ExperienceRowComponentView experienceRowPrefab;

        [Header("Prefab References")]
        [SerializeField] internal Button closeButton;
        [SerializeField] internal GridContainerComponentView availableExperiences;
        [SerializeField] internal ShowHideAnimator toastAnimator;
        [SerializeField] internal TMP_Text toastLabel;

        private Pool experiencesPool;
        private Coroutine toastRoutine;

        public Color rowsBackgroundColor;
        public Color rowsOnHoverColor;

        public event Action OnCloseButtonPressed;
        public event Action<string, bool> OnExperienceUiVisibilityChanged;
        public event Action<string, bool> OnExperienceExecutionChanged;

        public Transform ExperienceViewerTransform => transform;

        public static ExperiencesViewerComponentView Create()
        {
            ExperiencesViewerComponentView experiencesViewerComponentView = Instantiate(Resources.Load<GameObject>("ExperiencesViewer")).GetComponent<ExperiencesViewerComponentView>();
            experiencesViewerComponentView.name = "_ExperiencesViewer";

            return experiencesViewerComponentView;
        }

        public override void Awake()
        {
            base.Awake();

            closeButton.onClick.AddListener(() => OnCloseButtonPressed?.Invoke());

            ConfigurePool();
        }

        public override void RefreshControl()
        {
            availableExperiences.RefreshControl();
        }

        public void SetAvailableExperiences(List<ExperienceRowComponentModel> experiences)
        {
            availableExperiences.ExtractItems();
            experiencesPool.ReleaseAll();

            foreach (ExperienceRowComponentModel exp in experiences)
                AddAvailableExperience(exp);
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

        public void ShowUiHiddenToast(string pxName)
        {
            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
                toastRoutine = null;
            }

            pxName = string.IsNullOrEmpty(pxName) ? "Experience" : pxName;

            toastLabel.text = $"<b>{pxName}</b> UI hidden";
            toastRoutine = StartCoroutine(ShowToastRoutine());
        }

        public void ShowUiShownToast(string pxName)
        {
            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
                toastRoutine = null;
            }

            pxName = string.IsNullOrEmpty(pxName) ? "Experience" : pxName;

            toastLabel.text = $"<b>{pxName}</b> UI shown";
            toastRoutine = StartCoroutine(ShowToastRoutine());
        }

        public void ShowEnabledToast(string pxName)
        {
            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
                toastRoutine = null;
            }

            pxName = string.IsNullOrEmpty(pxName) ? "Experience" : pxName;

            toastLabel.text = $"<b>{pxName}</b> activated";
            toastRoutine = StartCoroutine(ShowToastRoutine());
        }

        public void ShowDisabledToast(string pxName)
        {
            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
                toastRoutine = null;
            }

            pxName = string.IsNullOrEmpty(pxName) ? "Experience" : pxName;

            toastLabel.text = $"<b>{pxName}</b> deactivated";
            toastRoutine = StartCoroutine(ShowToastRoutine());
        }

        public override void Dispose()
        {
            base.Dispose();

            closeButton.onClick.RemoveAllListeners();
        }

        private void OnSomeExperienceUIVisibilityChanged(string pexId, bool isUIVisible) =>
            OnExperienceUiVisibilityChanged?.Invoke(pexId, isUIVisible);

        private void OnSomeExperienceExecutionChanged(string pexId, bool isPlaying) =>
            OnExperienceExecutionChanged?.Invoke(pexId, isPlaying);

        private void ConfigurePool()
        {
            experiencesPool = PoolManager.i.GetPool(EXPERIENCES_POOL_NAME);

            if (experiencesPool != null) return;

            experiencesPool = PoolManager.i.AddPool(
                EXPERIENCES_POOL_NAME,
                Instantiate(experienceRowPrefab).gameObject,
                maxPrewarmCount: EXPERIENCES_POOL_PREWARM,
                isPersistent: true);

            experiencesPool.ForcePrewarm();
        }

        private IEnumerator ShowToastRoutine()
        {
            toastAnimator.Show();
            yield return new WaitForSeconds(UI_HIDDEN_TOAST_SHOWING_TIME);
            toastAnimator.Hide();
        }
    }
}
