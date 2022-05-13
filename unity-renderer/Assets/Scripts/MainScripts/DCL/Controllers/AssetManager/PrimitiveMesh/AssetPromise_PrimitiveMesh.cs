using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_PrimitiveMesh: AssetPromise<Asset_PrimitiveMesh>
    {
        public PrimitiveMeshModel model;

        private static readonly IPrimitiveMeshFactory primitiveMeshFactory = new PrimitiveMeshFactory();

        public AssetPromise_PrimitiveMesh(PrimitiveMeshModel model)
        {
            this.model = model;
        }
        
        protected override void OnAfterLoadOrReuse() {  }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnCancelLoading() {  }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            try
            {
                Mesh currentMesh = primitiveMeshFactory.CreateMesh(model);
                asset.mesh = currentMesh;
                OnSuccess?.Invoke();
            }
            catch(Exception e)
            {
                OnFail?.Invoke(e);
            }
        }

        public override object GetId() {  return model; }
    }
}