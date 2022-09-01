using System;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_PrimitiveMesh : AssetPromise<Asset_PrimitiveMesh>
    {
        private readonly AssetPromise_PrimitiveMesh_Model model;

        public AssetPromise_PrimitiveMesh(AssetPromise_PrimitiveMesh_Model model)
        {
            this.model = model;
        }

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnCancelLoading() { }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            try
            {
                Mesh currentMesh = PrimitiveMeshFactory.CreateMesh(model);
                asset.mesh = currentMesh;
                OnSuccess?.Invoke();
            }
            catch (Exception e)
            {
                OnFail?.Invoke(e);
            }
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                return false;
            }

            asset = library.Get(asset.id);
            return true;
        }

        public override object GetId()
        {
            return model;
        }
    }
}
