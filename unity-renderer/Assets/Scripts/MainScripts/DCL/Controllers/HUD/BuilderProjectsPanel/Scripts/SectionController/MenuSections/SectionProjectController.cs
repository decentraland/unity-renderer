using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Builder
{
    public interface ISectionProjectController
    {
        /// <summary>
        /// This event will be fired when the view request a new project creation
        /// </summary>
        event Action OnCreateProjectRequest;
    }
    
    internal class SectionProjectController : SectionBase, IProjectsListener, ISectionHideContextMenuRequester,ISectionProjectController
    {
        public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionProjectView";

        public event Action OnRequestContextMenuHide;
        public event Action OnCreateProjectRequest;

        public override ISectionSearchHandler searchHandler => sceneSearchHandler;

        private readonly SectionProjectView view;

        private readonly ISectionSearchHandler sceneSearchHandler = new SectionSearchHandler();
        internal Dictionary<string, IProjectCardView> projectsViews = new Dictionary<string, IProjectCardView>();

        public SectionProjectController() : this(
            Object.Instantiate(Resources.Load<SectionProjectView>(VIEW_PREFAB_PATH))
        ) { }

        public SectionProjectController(SectionProjectView view)
        {
            this.view = view;

            view.OnScrollRectValueChanged += OnRequestContextMenuHide;
            view.OnCreateProjectRequest += CreateProjectRequest;
            sceneSearchHandler.OnResult += OnSearchResult;
        }

        public override void SetViewContainer(Transform viewContainer) { view.SetParent(viewContainer); }

        public override void Dispose()
        {
            view.OnScrollRectValueChanged -= OnRequestContextMenuHide;
            view.OnCreateProjectRequest -= CreateProjectRequest;
            sceneSearchHandler.OnResult -= OnSearchResult;
            view.Dispose();
        }

        protected override void OnShow() { view.SetActive(true); }

        protected override void OnHide() { view.SetActive(false); }

        private void CreateProjectRequest()
        {
            OnCreateProjectRequest?.Invoke();
        }

        void IProjectsListener.OnSetProjects(Dictionary<string, IProjectCardView> projectsViews)
        {
            this.projectsViews = new Dictionary<string, IProjectCardView>(projectsViews);
            sceneSearchHandler.SetSearchableList(projectsViews.Values.Select(project => project.searchInfo).ToList());
        }

        void IProjectsListener.OnProjectAdded(IProjectCardView projectView)
        {
            projectsViews.Add(projectView.projectData.id, projectView);
            sceneSearchHandler.AddItem(projectView.searchInfo);
        }
        
        void IProjectsListener.OnProjectRemoved(IProjectCardView projectView)
        {
            projectsViews.Remove(projectView.projectData.id);
            projectView.SetActive(false);
        }

        internal void OnSearchResult(List<ISearchInfo> searchInfoScenes)
        {
            if (projectsViews == null)
                return;

            using (var iterator = projectsViews.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.SetParent(view.contentContainer.transform);
                    iterator.Current.Value.SetActive(false);
                }
            }

            for (int i = 0; i < searchInfoScenes.Count; i++)
            {
                if (!projectsViews.TryGetValue(searchInfoScenes[i].id, out IProjectCardView cardView))
                    continue;

                cardView.SetActive(true);
                cardView.SetSiblingIndex(i);
            }
            view.ResetScrollRect();

            if (projectsViews.Count == 0)
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