using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_BoxShape: AssetPromise<Asset_BoxShape>
    {
        public ECSBoxShape model;

        public AssetPromise_BoxShape(ECSBoxShape model)
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
                Mesh currentMesh = GenerateGeometry();
                Asset_BoxShape assetBoxShape = new Asset_BoxShape();
                assetBoxShape.cubeMesh = currentMesh;
                asset = assetBoxShape;
                OnSuccess?.Invoke();
            }
            catch(Exception e)
            {
                OnFail?.Invoke(e);
            }
        }

        public override object GetId() {  return model; }

        private Mesh GenerateGeometry()
        {
            Mesh  cubeMesh = PrimitiveMeshBuilder.BuildCube(1f);

            if (model.uvs != null && model.uvs.Length > 0)
            {
                cubeMesh.uv = Utils.FloatArrayToV2List(model.uvs);
            }
            
            return cubeMesh;
        }
    }
}