using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using Decentraland.Sdk.Ecs6;
using System;
using Object = UnityEngine.Object;

namespace DCL.Components
{
    public class BasicMaterial : BaseDisposable
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public string texture;

            // value that defines if a pixel is visible or invisible (no transparency gradients)
            [Range(0f, 1f)]
            public float alphaTest = 0.5f;

            public bool castShadows = true;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.BasicMaterial)
                    return Utils.SafeUnimplemented<BasicMaterial, Model>(expected: ComponentBodyPayload.PayloadOneofCase.BasicMaterial, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.BasicMaterial.HasTexture) pb.texture = pbModel.BasicMaterial.Texture;
                if (pbModel.BasicMaterial.HasAlphaTest) pb.alphaTest = pbModel.BasicMaterial.AlphaTest;
                if (pbModel.BasicMaterial.HasCastShadows) pb.castShadows = pbModel.BasicMaterial.CastShadows;

                return pb;
            }
        }

        public Material material;

        private DCLTexture dclTexture = null;

        private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int _AlphaClip = Shader.PropertyToID("_AlphaClip");
        private static readonly int _Cutoff = Shader.PropertyToID("_Cutoff");
        private static readonly int _ZWrite = Shader.PropertyToID("_ZWrite");

        private readonly DCLTexture.Fetcher dclTextureFetcher = new DCLTexture.Fetcher();
        private bool isDisposed;

        public BasicMaterial()
        {
            material = new Material(Utils.EnsureResourcesMaterial("Materials/BasicShapeMaterial"));

            OnAttach += OnMaterialAttached;
            OnDetach += OnMaterialDetached;
            model = new Model();
        }

        new public Model GetModel() { return (Model) model; }

        public override int GetClassId() { return (int) CLASS_ID.BASIC_MATERIAL; }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
                return;

            scene.componentsManagerLegacy.RemoveSharedComponent(entity, (typeof(PBRMaterial)));
            base.AttachTo(entity, overridenAttachedType);
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            base.DetachFrom(entity, overridenAttachedType);
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            if (material == null)
            {
                yield break; // We escape ApplyChanges called in the parent's constructor
            }

#if UNITY_EDITOR
            material.name = "BasicMaterial_" + id;
#endif

            Model model = (Model) newModel;

            if (!string.IsNullOrEmpty(model.texture))
            {
                if (dclTexture == null || dclTexture.id != model.texture)
                {
                    yield return dclTextureFetcher.Fetch(
                        scene,
                        model.texture,
                        texture =>
                        {
                            if (isDisposed)
                                return false;

                            if (dclTexture != null)
                            {
                                dclTexture.DetachFrom(this);
                            }

                            material.SetTexture(_BaseMap, texture.texture);
                            dclTexture = texture;
                            dclTexture.AttachTo(this);
                            return true;
                        }).ToCoroutine(e =>
                    {
                        if (e is not OperationCanceledException)
                            throw e;
                    });
                    // using `ToCoroutine()` since using Task directly arise some component lifecycle issues
                }
            }
            else
            {
                material.mainTexture = null;

                if ( dclTexture != null )
                {
                    dclTexture.DetachFrom(this);
                    dclTexture = null;
                }
            }

            material.EnableKeyword("_ALPHATEST_ON");
            material.SetInt(_ZWrite, 1);
            material.SetFloat(_AlphaClip, 1);
            material.SetFloat(_Cutoff, model.alphaTest);
            material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;

            foreach (IDCLEntity entity in attachedEntities)
            {
                InitMaterial(entity);
            }
        }

        void OnMaterialAttached(IDCLEntity entity)
        {
            entity.OnShapeUpdated -= OnShapeUpdated;
            entity.OnShapeUpdated += OnShapeUpdated;

            if (entity.meshRootGameObject != null)
            {
                var meshRenderer = entity.meshRootGameObject.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                    InitMaterial(entity);
            }
        }

        void InitMaterial(IDCLEntity entity)
        {
            var meshGameObject = entity.meshRootGameObject;

            if (meshGameObject == null)
                return;

            var meshRenderer = meshGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
                return;

            Model model = (Model) this.model;

            meshRenderer.shadowCastingMode = model.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;

            if (meshRenderer.sharedMaterial == material)
                return;

            SRPBatchingHelper.OptimizeMaterial(material);

            Material oldMaterial = meshRenderer.sharedMaterial;
            meshRenderer.sharedMaterial = material;

            DataStore.i.sceneWorldObjects.RemoveMaterial(scene.sceneData.sceneNumber, entity.entityId, oldMaterial);
            DataStore.i.sceneWorldObjects.AddMaterial(scene.sceneData.sceneNumber, entity.entityId, material);
        }

        private void OnShapeUpdated(IDCLEntity entity)
        {
            if (entity != null)
                InitMaterial(entity);
        }

        void OnMaterialDetached(IDCLEntity entity)
        {
            if (entity.meshRootGameObject == null)
                return;

            entity.OnShapeUpdated -= OnShapeUpdated;

            var meshRenderer = entity.meshRootGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer && meshRenderer.sharedMaterial == material)
                meshRenderer.sharedMaterial = null;

            DataStore.i.sceneWorldObjects.RemoveMaterial(scene.sceneData.sceneNumber, entity.entityId, material);
        }

        public override void Dispose()
        {
            isDisposed = true;

            dclTexture?.DetachFrom(this);

            while (attachedEntities != null && attachedEntities.Count > 0 )
            {
                DetachFrom(attachedEntities.First());
            }

            Object.Destroy(material);

            dclTextureFetcher.Dispose();
            base.Dispose();
        }
    }
}
