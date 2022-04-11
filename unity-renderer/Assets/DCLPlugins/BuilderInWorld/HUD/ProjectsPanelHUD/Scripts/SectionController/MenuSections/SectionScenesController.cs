using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Builder
{
    internal class SectionScenesController : SectionBase, ISceneListener, ISectionHideContextMenuRequester
    {
        public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionDeployedScenesView";

        public event Action OnRequestContextMenuHide;

        public override ISectionSearchHandler searchHandler => sceneSearchHandler;

        private readonly SectionPlacesView view;

        private readonly ISectionSearchHandler sceneSearchHandler = new SectionSearchHandler();
        private Dictionary<string, ISceneCardView> scenesViews = new Dictionary<string, ISceneCardView>();

        public SectionScenesController() : this(
            Object.Instantiate(Resources.Load<SectionPlacesView>(VIEW_PREFAB_PATH))
        ) { }

        public SectionScenesController(SectionPlacesView view)
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

        protected override void OnShow()
        {
            view.SetActive(true);
            if (scenesViews != null && scenesViews.Count == 0)
            {
                if (isLoading)
                {
                    view.SetLoading();
                    OnNotEmptyContent?.Invoke();
                }
                else
                {
                    view.SetEmpty();
                    OnEmptyContent?.Invoke();
                }
            }
            else
            {
                OnNotEmptyContent?.Invoke();
            }
        }

        protected override void OnHide() { view.SetActive(false); }

        void ISceneListener.SetScenes(Dictionary<string, ISceneCardView> scenes)
        {
            scenesViews = new Dictionary<string, ISceneCardView>(scenes);
            sceneSearchHandler.SetSearchableList(scenes.Values.Select(scene => scene.searchInfo).ToList());
        }

        void ISceneListener.SceneAdded(ISceneCardView scene)
        {
            scenesViews.Add(scene.SceneData.id, scene);
            sceneSearchHandler.AddItem(scene.searchInfo);
        }

        void ISceneListener.SceneRemoved(ISceneCardView scene)
        {
            scenesViews.Remove(scene.SceneData.id);
            scene.SetActive(false);
        }

        private void OnSearchResult(List<ISearchInfo> searchInfoScenes)
        {
            if (scenesViews == null || searchInfoScenes == null)
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
                if (!scenesViews.TryGetValue(searchInfoScenes[i].id, out ISceneCardView cardView))
                    continue;

                cardView.SetActive(true);
                cardView.SetSiblingIndex(i);
            }

            if (scenesViews.Count == 0)
            {
                if (isLoading)
                {
                    view.SetLoading();
                    OnNotEmptyContent?.Invoke();
                }
                else
                {
                    view.SetEmpty();
                    OnEmptyContent?.Invoke();
                }
            }
            else if (searchInfoScenes.Count == 0)
            {
                view.SetNoSearchResult();
                OnNotEmptyContent?.Invoke();
            }
            else
            {
                view.SetFilled();
                OnNotEmptyContent?.Invoke();
            }
        }
    }
}