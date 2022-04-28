using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_Mesh: AssetPromise<Asset_Mesh>
    {
        public ECSBoxShape model;

        public AssetPromise_Mesh(ECSBoxShape model)
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
                Asset_Mesh assetMesh = new Asset_Mesh();
                assetMesh.cubeMesh = currentMesh;
                asset = assetMesh;
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
            Mesh cubeMesh = PrimitiveMeshBuilder.BuildCube(1f);

            if (model.uvs != null && model.uvs.Length > 0)
            {
                cubeMesh.uv = Utils.FloatArrayToV2List(model.uvs);
            }
            
            return cubeMesh;
        }
    }
}