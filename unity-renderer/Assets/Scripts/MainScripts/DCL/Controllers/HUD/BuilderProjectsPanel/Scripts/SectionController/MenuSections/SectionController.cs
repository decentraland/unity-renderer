using System;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Builder
{
    internal class SectionController : SectionBase, IPlaceListener, IProjectListener, ISectionOpenSectionRequester
    {
        public event Action<SectionId> OnRequestOpenSection;

        internal const int MAX_CARDS = 3;
        internal readonly SectionScenesView view;

        private bool hasScenes = false;

        public override ISectionSearchHandler searchHandler => hasScenes ? sceneSearchHandler : null;
        public override SearchBarConfig searchBarConfig => new SearchBarConfig()
        {
            showFilterContributor = false,
            showFilterOperator = false,
            showFilterOwner = false,
            showResultLabel = false
        };

        private readonly ISectionSearchHandler sceneSearchHandler = new SectionSearchHandler();

        internal Dictionary<string, IPlaceCardView> deployedViews;
        internal Dictionary<string, IPlaceCardView> projectViews;
        private List<ISearchInfo> searchList = new List<ISearchInfo>();

        public SectionController()
        {
            var prefab = Resources.Load<SectionScenesView>("BuilderProjectsPanelMenuSections/SectionScenesView");
            view = Object.Instantiate(prefab);

            view.btnProjectsViewAll.onClick.AddListener(() => OnRequestOpenSection?.Invoke(SectionId.PROJECTS));
            view.btnInWorldViewAll.onClick.AddListener(() => OnRequestOpenSection?.Invoke(SectionId.SCENES));

            sceneSearchHandler.OnResult += OnSearchResult;
        }

        public override void SetViewContainer(Transform viewContainer)
        {
            view.transform.SetParent(viewContainer);
            view.transform.ResetLocalTRS();
        }

        public override void Dispose() { view.Dispose(); }

        protected override void OnShow() { view.gameObject.SetActive(true); }

        protected override void OnHide()
        {
            view.gameObject.SetActive(false);
            searchList.Clear();
            deployedViews?.Clear();
            projectViews?.Clear();
        }

        private void ViewDirty()
        {
            bool hasDeployedScenes = view.deployedSceneContainer.childCount > 0;
            bool hasProjectScenes = view.projectSceneContainer.childCount > 0;
            hasScenes = hasDeployedScenes || hasProjectScenes;

            view.contentScreen.SetActive(hasScenes);
            view.emptyScreen.SetActive(!hasScenes);
            view.inWorldContainer.SetActive(hasDeployedScenes);
            view.projectsContainer.SetActive(hasProjectScenes);
        }

        void IPlaceListener.SetPlaces(Dictionary<string, IPlaceCardView> scenes)
        {
            UpdateDictionary(ref deployedViews, scenes);
            searchList.AddRange(scenes.Values.Select(scene => scene.searchInfo));
            sceneSearchHandler.SetSearchableList(searchList);
        }

        void IProjectListener.SetPlaces(Dictionary<string, IPlaceCardView> scenes)
        {
            UpdateDictionary(ref projectViews, scenes);
            searchList.AddRange(scenes.Values.Select(scene => scene.searchInfo));
            sceneSearchHandler.SetSearchableList(searchList);
        }

        void IPlaceListener.PlaceAdded(IPlaceCardView place)
        {
            deployedViews.Add(place.PlaceData.id, place);
            sceneSearchHandler.AddItem(place.searchInfo);
        }

        void IProjectListener.PlaceAdded(IPlaceCardView place)
        {
            projectViews.Add(place.PlaceData.id, place);
            sceneSearchHandler.AddItem(place.searchInfo);
        }

        void IPlaceListener.PlaceRemoved(IPlaceCardView place)
        {
            place.SetToDefaultParent();
            deployedViews.Remove(place.PlaceData.id);
            sceneSearchHandler.RemoveItem(place.searchInfo);
        }

        void IProjectListener.PlaceRemoved(IPlaceCardView place)
        {
            place.SetToDefaultParent();
            projectViews.Remove(place.PlaceData.id);
            sceneSearchHandler.RemoveItem(place.searchInfo);
        }

        private void OnSearchResult(List<ISearchInfo> searchInfoScenes)
        {
            if (deployedViews != null)
                SetResult(deployedViews, searchInfoScenes, view.deployedSceneContainer);

            if (projectViews != null)
                SetResult(projectViews, searchInfoScenes, view.projectSceneContainer);

            ViewDirty();
        }

        private void SetResult(Dictionary<string, IPlaceCardView> scenesViews, List<ISearchInfo> searchInfoScenes,
            Transform parent)
        {
            int count = 0;

            for (int i = 0; i < searchInfoScenes.Count; i++)
            {
                if (!scenesViews.TryGetValue(searchInfoScenes[i].id, out IPlaceCardView sceneView))
                {
                    continue;
                }

                sceneView.SetParent(parent);
                sceneView.SetSiblingIndex(count);
                sceneView.SetActive(false);
                count++;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(i < count && i < MAX_CARDS);
            }
        }

        private void UpdateDictionary(ref Dictionary<string, IPlaceCardView> target, Dictionary<string, IPlaceCardView> newData)
        {
            if (newData.Count == 0)
                return;

            if (target == null)
            {
                target = new Dictionary<string, IPlaceCardView>(newData);
                return;
            }

            using (var iterator = newData.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (target.ContainsKey(iterator.Current.Key))
                        continue;

                    target.Add(iterator.Current.Key, iterator.Current.Value);
                }
            }
        }
    }
}