using System;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class NftBreadcrumbComponentView : BaseComponentView<NftBreadcrumbModel>
    {
        [SerializeField] internal NftSubCategoryFilterComponentView prefab;
        [SerializeField] internal RectTransform container;
        [SerializeField] internal NFTTypeIconsAndColors iconsByCategory;

        private readonly Dictionary<NftSubCategoryFilterComponentView, PoolableObject> pooledObjects = new ();
        private NftSubCategoryFilterComponentView[] categoriesByIndex = Array.Empty<NftSubCategoryFilterComponentView>();
        private Pool pool;

        internal NftBreadcrumbModel Model => model;

        public event Action<string> OnFilterSelected;
        public event Action<string> OnFilterRemoved;

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
                view.OnNavigate -= ApplyFilter;
                view.OnExit -= RemoveFilter;
            }

            pooledObjects.Clear();

            categoriesByIndex = new NftSubCategoryFilterComponentView[model.Path.Length];
            var i = 0;

            foreach ((string Filter, string Name, string Type, bool Removable) subCategory in model.Path)
            {
                PoolableObject poolObj = pool.Get();
                NftSubCategoryFilterComponentView view = poolObj.gameObject.GetComponent<NftSubCategoryFilterComponentView>();

                bool isLastItem = i == model.Path.Length - 1;
                Sprite icon = iconsByCategory.GetTypeImage(subCategory.Type);

                view.SetModel(new NftSubCategoryFilterModel
                {
                    Name = subCategory.Name,
                    Filter = subCategory.Filter,
                    ResultCount = model.ResultCount,
                    ShowResultCount = isLastItem,
                    Icon = icon,
                    IsSelected = model.Current == i,
                    ShowRemoveButton = subCategory.Removable,
                });

                view.OnNavigate += ApplyFilter;
                view.OnExit += RemoveFilter;
                view.transform.SetParent(container, false);

                pooledObjects[view] = poolObj;
                categoriesByIndex[i++] = view;
            }
        }

        private void RemoveFilter(NftSubCategoryFilterModel model) =>
            OnFilterRemoved?.Invoke(model.Filter);

        private void ApplyFilter(NftSubCategoryFilterModel model) =>
            OnFilterSelected?.Invoke(model.Filter);
    }
}
