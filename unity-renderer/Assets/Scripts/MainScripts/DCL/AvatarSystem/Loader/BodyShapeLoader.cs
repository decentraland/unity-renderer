using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AvatarSystem
{
    public class BodyShapeLoader : IBodyshapeLoader
    {
        public WearableItem wearable { get; }
        public Rendereable rendereable => bodyshapeRetriever?.rendereable;
        public IWearableLoader.Status status { get; private set; }
        public WearableItem eyes { get; }
        public WearableItem eyebrows { get; }
        public WearableItem mouth { get; }
        public SkinnedMeshRenderer eyesRenderer { get; private set; }
        public SkinnedMeshRenderer eyebrowsRenderer { get; private set; }
        public SkinnedMeshRenderer mouthRenderer { get; private set; }
        public SkinnedMeshRenderer headRenderer { get; private set; }
        public SkinnedMeshRenderer feetRenderer { get; private set; }
        public SkinnedMeshRenderer upperBodyRenderer { get; private set; }
        public SkinnedMeshRenderer lowerBodyRenderer { get; private set; }

        internal readonly IWearableRetriever bodyshapeRetriever;
        internal readonly IFacialFeatureRetriever eyesRetriever;
        internal readonly IFacialFeatureRetriever eyebrowsRetriever;
        internal readonly IFacialFeatureRetriever mouthRetriever;

        public BodyShapeLoader(IRetrieverFactory retrieverFactory, WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth)
        {
            wearable = bodyshape;
            this.eyes = eyes;
            this.eyebrows = eyebrows;
            this.mouth = mouth;

            bodyshapeRetriever = retrieverFactory.GetWearableRetriever();
            eyesRetriever = retrieverFactory.GetFacialFeatureRetriever();
            eyebrowsRetriever = retrieverFactory.GetFacialFeatureRetriever();
            mouthRetriever = retrieverFactory.GetFacialFeatureRetriever();
        }

        public async UniTask Load(GameObject container, AvatarSettings avatarSettings, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                if (status == IWearableLoader.Status.Succeeded)
                {
                    UpdateColors(avatarSettings);
                    return;
                }

                status = IWearableLoader.Status.Idle;

                await LoadWearable(container, ct);

                (headRenderer, upperBodyRenderer, lowerBodyRenderer, feetRenderer, eyesRenderer, eyebrowsRenderer, mouthRenderer) = AvatarSystemUtils.ExtractBodyshapeParts(bodyshapeRetriever.rendereable);
               
                await (LoadEyes(ct), LoadEyebrows(ct), LoadMouth(ct));
               
                UpdateColors(avatarSettings);
                status = IWearableLoader.Status.Succeeded;
            }
            catch (Exception)
            {
                status = IWearableLoader.Status.Failed;
                Dispose();
                throw;
            }
        }

        private async UniTask LoadMouth(CancellationToken ct)
        {
            if (mouth == null) return;
            (Texture main, Texture mask) = await mouthRetriever.Retrieve(mouth, wearable.id, ct);
            mouthRenderer.material = new Material(Resources.Load<Material>("Mouth Material"));
            if (main == null)
                throw new Exception($"Couldn't fetch main texture for {mouth.id}");
            mouthRenderer.material.SetTexture(ShaderUtils.BaseMap, main);
            if (mask != null)
                mouthRenderer.material.SetTexture(ShaderUtils.TintMask, mask);
        }

        private async UniTask LoadEyebrows(CancellationToken ct)
        {
            if (eyebrows == null) return;
            (Texture main, Texture mask) = await eyebrowsRetriever.Retrieve(eyebrows, wearable.id, ct);
            eyebrowsRenderer.material = new Material(Resources.Load<Material>("Eyebrow Material"));
            if (main == null)
                throw new Exception($"Couldn't fetch main texture for {eyebrows.id}");
            eyebrowsRenderer.material.SetTexture(ShaderUtils.BaseMap, main);
            if (mask != null)
                eyebrowsRenderer.material.SetTexture(ShaderUtils.BaseMap, mask);
        }

        private async UniTask LoadEyes(CancellationToken ct)
        {
            if (eyes == null) return;
            (Texture main, Texture mask) = await eyesRetriever.Retrieve(eyes, wearable.id, ct);
            eyesRenderer.material = new Material(Resources.Load<Material>("Eye Material"));
            if (main == null)
                throw new Exception($"Couldn't fetch main texture for {eyes.id}");
            eyesRenderer.material.SetTexture(ShaderUtils.EyesTexture, main);
            if (mask != null)
                eyesRenderer.material.SetTexture(ShaderUtils.IrisMask, mask);
        }

        private void UpdateColors(AvatarSettings avatarSettings)
        {
            AvatarSystemUtils.PrepareMaterialColors(rendereable, avatarSettings.skinColor, avatarSettings.hairColor);
            eyesRenderer?.material?.SetColor(ShaderUtils.EyeTint, avatarSettings.eyesColor);
            eyebrowsRenderer?.material?.SetColor(ShaderUtils.BaseColor, avatarSettings.hairColor);
            mouthRenderer?.material?.SetColor(ShaderUtils.BaseColor, avatarSettings.skinColor);
        }

        private async UniTask<Rendereable> LoadWearable(GameObject container, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            bodyshapeRetriever.Dispose();

            WearableItem.Representation representation = wearable.GetRepresentation(wearable.id);
            if (representation == null)
                throw new Exception("Couldn't find a representation for this bodyshape");

            Rendereable bodyshapeRenderable = await bodyshapeRetriever.Retrieve(container, wearable.GetContentProvider(wearable.id), wearable.baseUrlBundles, representation.mainFile, ct);

            if (bodyshapeRenderable == null) // fail safe, we shouldnt reach this since .Retrieve should throw if anything goes wrong
                throw new Exception("Couldn't load bodyshape");

            return bodyshapeRenderable;
        }
        
        public bool IsValid(WearableItem bodyshape, WearableItem eyebrows, WearableItem eyes, WearableItem mouth)
        {
            if (wearable.id != bodyshape?.id) return false;
            if (this.eyebrows?.id != eyebrows?.id) return false;
            if (this.mouth?.id != mouth?.id) return false;
            if (this.eyes?.id != eyes?.id) return false;
            return true;
        }

        public void Dispose()
        {
            status = IWearableLoader.Status.Idle;
            bodyshapeRetriever?.Dispose();
            eyesRetriever?.Dispose();
            eyebrowsRetriever?.Dispose();
            mouthRetriever?.Dispose();

            if (eyesRenderer != null)
                Object.Destroy(eyesRenderer.material);
            if (eyebrowsRenderer != null)
                Object.Destroy(eyebrowsRenderer.material);
            if (mouthRenderer != null)
                Object.Destroy(mouthRenderer.material);

            upperBodyRenderer = null;
            lowerBodyRenderer = null;
            feetRenderer = null;
            headRenderer = null;
            eyesRenderer = null;
            eyebrowsRenderer = null;
            mouthRenderer = null;
        }
    }
}