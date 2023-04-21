using System;
using System.Collections.Generic;
using System.Linq;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL.Backpack
{
    public class NftBreadcrumbComponentView : BaseComponentView<NftBreadcrumbModel>
    {
        [Serializable]
        internal struct FilterIconType
        {
            public string filterPrefix;
            public Sprite icon;
        }

        [SerializeField] internal NftSubCategoryFilterComponentView prefab;
        [SerializeField] internal RectTransform container;
        [SerializeField] internal FilterIconType[] iconsByFilter;

        private readonly Dictionary<NftSubCategoryFilterComponentView, PoolableObject> pooledObjects = new ();
        private NftSubCategoryFilterComponentView[] categoriesByIndex = Array.Empty<NftSubCategoryFilterComponentView>();
        private Pool pool;

        internal NftBreadcrumbModel Model => model;

        public event Action<string> OnNavigate;

        public override void Awake()
        {
            base.Awake();

            pool = PoolManager.i.AddPool(
                $"NftBreadcrumbComponentView_{GetInstanceID()}",
                Instantiate(prefab).gameObject,
                maxPrewarmCount: 5,
                isPersistent: true);

            pool.ForcePrewarm();
        }

        public override void RefreshControl()
        {
            foreach ((NftSubCategoryFilterComponentView view, PoolableObject poolObj) in pooledObjects)
            {
                poolObj.Release();
                view.OnNavigate -= Navigate;
                view.OnExit -= NavigateToPreviousCategory;
            }

            pooledObjects.Clear();

            categoriesByIndex = new NftSubCategoryFilterComponentView[model.Path.Length];
            var i = 0;

            foreach ((string Filter, string Name) subCategory in model.Path)
            {
                PoolableObject poolObj = pool.Get();
                NftSubCategoryFilterComponentView view = poolObj.gameObject.GetComponent<NftSubCategoryFilterComponentView>();

                bool isLastItem = i == model.Path.Length - 1;
                FilterIconType filterIcon = iconsByFilter.FirstOrDefault(o => subCategory.Filter.StartsWith(o.filterPrefix));

                view.SetModel(new NftSubCategoryFilterModel
                {
                    Name = subCategory.Name,
                    Filter = subCategory.Filter,
                    ResultCount = model.ResultCount,
                    ShowResultCount = isLastItem,
                    Icon = filterIcon.icon,
                    IsSelected = model.Current == i,
                });

                view.OnNavigate += Navigate;
                view.OnExit += NavigateToPreviousCategory;
                view.transform.SetParent(container, false);

                pooledObjects[view] = poolObj;
                categoriesByIndex[i++] = view;
            }
        }

        private void NavigateToPreviousCategory(NftSubCategoryFilterModel model)
        {
            int index = Array.FindIndex(this.model.Path, tuple => tuple.Filter == model.Filter);
            Navigate(categoriesByIndex[index].Model);
        }

        private void Navigate(NftSubCategoryFilterModel model) =>
            OnNavigate?.Invoke(model.Filter);
    }
}
