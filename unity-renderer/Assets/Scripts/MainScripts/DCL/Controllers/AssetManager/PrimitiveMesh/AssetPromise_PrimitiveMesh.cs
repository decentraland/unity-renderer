using System;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_PrimitiveMesh : AssetPromise<Asset_PrimitiveMesh>
    {
        public PrimitiveMeshModel model;

        private static readonly IPrimitiveMeshFactory primitiveMeshFactory = new PrimitiveMeshFactory();

        public AssetPromise_PrimitiveMesh(PrimitiveMeshModel model)
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
                Mesh currentMesh = primitiveMeshFactory.CreateMesh(model);
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
