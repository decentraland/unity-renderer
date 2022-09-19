using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    /// <summary>
    /// Hide white images when switching to NavMap.
    /// FIX issue #2071 <see cref="https://github.com/decentraland/unity-renderer/issues/2071"/>
    /// </summary>
    public class NavMapLoadingLogo : MonoBehaviour
    {
        [SerializeField] private Image sectionsContent;
        [SerializeField] private Image radialGradient;

        [SerializeField] private GameObject navMapSection;

        private NavmapView navMap;

        private void Awake()
        {
            DataStore.i.HUDs.navmapVisible.OnChange += OnOpeningNavMap;
            DataStore.i.exploreV2.isOpen.OnChange += OnExploreUiVisibilityChange;
        }

        private void Start()
        {

            navMap = navMapSection.GetComponentInChildren<NavmapView>();
            navMap.OnToggle += OnNavMapLoaded;
        }

        private void OnDestroy()
        {
            DataStore.i.HUDs.navmapVisible.OnChange -= OnOpeningNavMap;
            DataStore.i.exploreV2.isOpen.OnChange -= OnExploreUiVisibilityChange;

            navMap.OnToggle -= OnNavMapLoaded;
        }

        private void OnExploreUiVisibilityChange(bool isOpen, bool _)
        {
            if (!isOpen && !sectionsContent.enabled)
            {
                SetImagesVisibility(visible: true);
            }
        }

        private void OnOpeningNavMap(bool isShown, bool _)
        {
            if (isShown)
            {
                SetImagesVisibility(visible: false);
            }
        }

        private void OnNavMapLoaded(bool _) =>
            SetImagesVisibility(visible: true);

        private void SetImagesVisibility(bool visible)
        {
            sectionsContent.enabled = visible;
            radialGradient.enabled = visible;
        }
    }
}