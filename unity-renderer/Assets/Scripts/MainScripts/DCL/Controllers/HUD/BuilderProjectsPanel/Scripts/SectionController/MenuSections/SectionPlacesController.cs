using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Builder
{
    internal class SectionPlacesController : SectionBase, IPlaceListener, ISectionHideContextMenuRequester
    {
        public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionDeployedScenesView";

        public event Action OnRequestContextMenuHide;

        public override ISectionSearchHandler searchHandler => sceneSearchHandler;

        private readonly SectionPlacesView view;

        private readonly ISectionSearchHandler sceneSearchHandler = new SectionSearchHandler();
        private Dictionary<string, IPlaceCardView> scenesViews;

        public SectionPlacesController() : this(
            Object.Instantiate(Resources.Load<SectionPlacesView>(VIEW_PREFAB_PATH))
        ) { }

        public SectionPlacesController(SectionPlacesView view)
        {
            this.view = view;

            view.OnScrollRectValueChanged += OnRequestContextMenuHide;
            sceneSearchHandler.OnResult += OnSearchResult;
        }

        public override void SetViewContainer(Transform viewContainer) { view.SetParent(viewContainer); }

        public override void Dispose()
        {
            view.OnScrollRectValueChanged -= OnRequestContextMenuHide;
            view.Dispose();
        }

        protected override void OnShow() { view.SetActive(true); }

        protected override void OnHide() { view.SetActive(false); }

        void IPlaceListener.SetPlaces(Dictionary<string, IPlaceCardView> scenes)
        {
            scenesViews = new Dictionary<string, IPlaceCardView>(scenes);
            sceneSearchHandler.SetSearchableList(scenes.Values.Select(scene => scene.searchInfo).ToList());
        }

        void IPlaceListener.PlaceAdded(IPlaceCardView place)
        {
            scenesViews.Add(place.PlaceData.id, place);
            sceneSearchHandler.AddItem(place.searchInfo);
        }

        void IPlaceListener.PlaceRemoved(IPlaceCardView place)
        {
            scenesViews.Remove(place.PlaceData.id);
            place.SetActive(false);
        }

        private void OnSearchResult(List<ISearchInfo> searchInfoScenes)
        {
            if (scenesViews == null)
                return;

            using (var iterator = scenesViews.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.SetParent(view.GetCardsContainer());
                    iterator.Current.Value.SetActive(false);
                }
            }

            for (int i = 0; i < searchInfoScenes.Count; i++)
            {
                if (!scenesViews.TryGetValue(searchInfoScenes[i].id, out IPlaceCardView cardView))
                    continue;

                cardView.SetActive(true);
                cardView.SetSiblingIndex(i);
            }

            if (scenesViews.Count == 0)
            {
                if (isLoading)
                {
                    view.SetLoading();
                }
                else
                {
                    view.SetEmpty();
                }
            }
            else if (searchInfoScenes.Count == 0)
            {
                view.SetNoSearchResult();
            }
            else
            {
                view.SetFilled();
            }
        }
    }
}