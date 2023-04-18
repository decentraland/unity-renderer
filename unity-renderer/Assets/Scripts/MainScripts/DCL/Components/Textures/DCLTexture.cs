using Cysharp.Threading.Tasks;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class DCLTexture : BaseDisposable
    {
        [Serializable]
        public class Model : BaseModel
        {
            public string src;
            public BabylonWrapMode wrap = BabylonWrapMode.CLAMP;
            public FilterMode samplingMode = FilterMode.Bilinear;
            public bool hasAlpha = false;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        public enum BabylonWrapMode
        {
            CLAMP,
            WRAP,
            MIRROR
        }

        AssetPromise_Texture texturePromise = null;

        protected Dictionary<ISharedComponent, HashSet<long>> attachedEntitiesByComponent =
            new Dictionary<ISharedComponent, HashSet<long>>();

        public TextureWrapMode unityWrap;
        public FilterMode unitySamplingMode;
        public Texture2D texture;

        protected bool isDisposed;
        protected bool textureDisposed;

        public float resizingFactor => texturePromise?.asset.resizingFactor ?? 1;

        public override int GetClassId()
        {
            return (int)CLASS_ID.TEXTURE;
        }

        public DCLTexture()
        {
            model = new Model();
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDisposed)
                yield break;

            Model model = (Model)newModel;

            unitySamplingMode = model.samplingMode;

            switch (model.wrap)
            {
                case BabylonWrapMode.CLAMP:
                    unityWrap = TextureWrapMode.Clamp;
                    break;
                case BabylonWrapMode.WRAP:
                    unityWrap = TextureWrapMode.Repeat;
                    break;
                case BabylonWrapMode.MIRROR:
                    unityWrap = TextureWrapMode.Mirror;
                    break;
            }

            if (texture == null && !string.IsNullOrEmpty(model.src))
            {
                bool isBase64 = model.src.Contains("image/png;base64");

                if (isBase64)
                {
                    string base64Data = model.src.Substring(model.src.IndexOf(',') + 1);

                    // The used texture variable can't be null for the ImageConversion.LoadImage to work
                    if (texture == null)
                    {
                        texture = new Texture2D(1, 1);
                    }

                    if (!ImageConversion.LoadImage(texture, Convert.FromBase64String(base64Data)))
                    {
                        Debug.LogError($"DCLTexture with id {id} couldn't parse its base64 image data.");
                    }

                    if (texture != null)
                    {
                        texture.wrapMode = unityWrap;
                        texture.filterMode = unitySamplingMode;

                        if (DataStore.i.textureConfig.runCompression.Get())
                            texture.Compress(false);

                        texture.Apply(unitySamplingMode != FilterMode.Point, true);
                        texture = TextureHelpers.ClampSize(texture, DataStore.i.textureConfig.generalMaxSize.Get());
                    }
                }
                else
                {
                    string contentsUrl = string.Empty;
                    bool isExternalURL = model.src.Contains("http://") || model.src.Contains("https://");

                    if (isExternalURL)
                        contentsUrl = model.src;
                    else
                        scene.contentProvider.TryGetContentsUrl(model.src, out contentsUrl);

                    var prevPromise = texturePromise;

                    if (!string.IsNullOrEmpty(contentsUrl))
                    {
                        if (texturePromise != null)
                            AssetPromiseKeeper_Texture.i.Forget(texturePromise);

                        texturePromise = new AssetPromise_Texture(contentsUrl, unityWrap, unitySamplingMode, storeDefaultTextureInAdvance: true);
                        texturePromise.OnSuccessEvent += (x) => texture = x.texture;
                        texturePromise.OnFailEvent += (x, error) => { texture = null; };

                        AssetPromiseKeeper_Texture.i.Keep(texturePromise);
                        yield return texturePromise;
                    }

                    AssetPromiseKeeper_Texture.i.Forget(prevPromise);
                }
            }
        }

        public virtual void AttachTo(ISharedComponent component)
        {
            AddReference(component);
        }

        public virtual void DetachFrom(ISharedComponent component)
        {
            if (RemoveReference(component))
            {
                if (attachedEntitiesByComponent.Count == 0)
                {
                    DisposeTexture();
                }
            }
        }

        public void AddReference(ISharedComponent component)
        {
            if (attachedEntitiesByComponent.ContainsKey(component))
                return;

            attachedEntitiesByComponent.Add(component, new HashSet<long>());

            foreach (var entity in component.GetAttachedEntities())
            {
                attachedEntitiesByComponent[component].Add(entity.entityId);
                DataStore.i.sceneWorldObjects.AddTexture(scene.sceneData.sceneNumber, entity.entityId, texture);
            }
        }

        public bool RemoveReference(ISharedComponent component)
        {
            if (!attachedEntitiesByComponent.ContainsKey(component))
                return false;

            foreach (var entityId in attachedEntitiesByComponent[component])
            {
                DataStore.i.sceneWorldObjects.RemoveTexture(scene.sceneData.sceneNumber, entityId, texture);
            }

            return attachedEntitiesByComponent.Remove(component);
        }

        public override void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            while (attachedEntitiesByComponent.Count > 0)
            {
                RemoveReference(attachedEntitiesByComponent.First().Key);
            }

            DisposeTexture();

            base.Dispose();
        }

        protected virtual void DisposeTexture()
        {
            textureDisposed = true;

            if (texturePromise != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                texturePromise = null;
            }
            else if (texture)
            {
                Object.Destroy(texture);
            }
        }

        public class Fetcher : IDisposable
        {
            private CancellationTokenSource cancellationTokenSource;

            public async UniTask Fetch(IParcelScene scene, string componentId,
                Func<DCLTexture, bool> attachCallback)
            {
                Cancel();

                cancellationTokenSource = new CancellationTokenSource();
                CancellationToken ct = cancellationTokenSource.Token;

                if (!scene.componentsManagerLegacy.HasSceneSharedComponent(componentId))
                {
                    Debug.Log($"couldn't fetch texture, the DCLTexture component with id {componentId} doesn't exist");
                    return;
                }

                DCLTexture textureComponent = scene.componentsManagerLegacy.GetSceneSharedComponent(componentId) as DCLTexture;

                if (textureComponent == null)
                {
                    Debug.Log($"couldn't fetch texture, the shared component with id {componentId} is NOT a DCLTexture");
                    return;
                }

                bool textureWasReLoaded = false;

                // If texture was previously disposed we load it again
                if (textureComponent.textureDisposed)
                {
                    textureComponent.textureDisposed = false;

                    try
                    {
                        await textureComponent.ApplyChanges(textureComponent.model).WithCancellation(ct);
                        ct.ThrowIfCancellationRequested();
                        textureWasReLoaded = true;
                    }
                    catch (OperationCanceledException _)
                    {
                        textureComponent.DisposeTexture();
                    }
                }

                await UniTask.WaitUntil(() => textureComponent.texture, PlayerLoopTiming.Update, ct);

                // We dispose texture was re-loaded but attachment
                // was unsuccessful
                if (!attachCallback(textureComponent) && textureWasReLoaded)
                {
                    textureComponent.DisposeTexture();
                }

                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            public void Cancel()
            {
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                }
            }

            public void Dispose()
            {
                Cancel();
            }
        }
    }
}
