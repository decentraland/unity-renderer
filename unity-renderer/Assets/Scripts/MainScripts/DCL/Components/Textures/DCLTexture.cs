using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DCL
{
    public class DCLTexture : BaseDisposable
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public string src;
            public BabylonWrapMode wrap = BabylonWrapMode.CLAMP;
            public FilterMode samplingMode = FilterMode.Bilinear;
            public bool hasAlpha = false;

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
        }

        public enum BabylonWrapMode
        {
            CLAMP,
            WRAP,
            MIRROR
        }

        AssetPromise_Texture texturePromise = null;

        private Dictionary<ISharedComponent, HashSet<long>> attachedEntitiesByComponent =
            new Dictionary<ISharedComponent, HashSet<long>>();

        public TextureWrapMode unityWrap;
        public FilterMode unitySamplingMode;
        public Texture2D texture;
        protected bool isDisposed;
        public float resizingFactor => texturePromise?.asset.resizingFactor ?? 1;

        public override int GetClassId() { return (int) CLASS_ID.TEXTURE; }

        public DCLTexture() { model = new Model(); }

        public static IEnumerator FetchFromComponent(IParcelScene scene, string componentId,
            System.Action<Texture2D> OnFinish)
        {
            yield return FetchTextureComponent(scene, componentId, (dclTexture) => { OnFinish?.Invoke(dclTexture.texture); });
        }

        public static IEnumerator FetchTextureComponent(IParcelScene scene, string componentId,
            System.Action<DCLTexture> OnFinish)
        {
            if (!scene.componentsManagerLegacy.HasSceneSharedComponent(componentId))
            {
                Debug.Log($"couldn't fetch texture, the DCLTexture component with id {componentId} doesn't exist");
                yield break;
            }

            DCLTexture textureComponent = scene.componentsManagerLegacy.GetSceneSharedComponent(componentId) as DCLTexture;

            if (textureComponent == null)
            {
                Debug.Log($"couldn't fetch texture, the shared component with id {componentId} is NOT a DCLTexture");
                yield break;
            }

            yield return new WaitUntil(() => textureComponent.texture != null);

            OnFinish.Invoke(textureComponent);
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDisposed)
                yield break;

            Model model = (Model) newModel;

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
                        texture.Compress(false);
                        texture.Apply(unitySamplingMode != FilterMode.Point, true);
                        texture = TextureHelpers.ClampSize(texture, DataStore.i.textureSize.generalMaxSize.Get());
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
                }
            }
        }

        public virtual void AttachTo(ISharedComponent component)
        {
            AddReference(component);
        }

        public virtual void DetachFrom(ISharedComponent component)
        {
            RemoveReference(component);
        }

        public void AddReference(ISharedComponent component)
        {
            if (attachedEntitiesByComponent.ContainsKey(component))
                return;

            attachedEntitiesByComponent.Add(component, new HashSet<long>());

            foreach (var entity in component.GetAttachedEntities())
            {
                attachedEntitiesByComponent[component].Add(entity.entityId);
                DataStore.i.sceneWorldObjects.AddTexture(scene.sceneData.id, entity.entityId, texture);
            }
        }

        public void RemoveReference(ISharedComponent component)
        {
            if (!attachedEntitiesByComponent.ContainsKey(component))
                return;

            foreach (var entityId in attachedEntitiesByComponent[component])
            {
                DataStore.i.sceneWorldObjects.RemoveTexture(scene.sceneData.id, entityId, texture);
            }

            attachedEntitiesByComponent.Remove(component);
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

            if (texturePromise != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                texturePromise = null;
            }

            base.Dispose();
        }
    }
}