using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;
using UnityEngine.Assertions;

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

        private readonly IWearableRetriever bodyshapeRetriever;
        private readonly IFacialFeatureRetriever eyesRetriever;
        private readonly IFacialFeatureRetriever eyebrowsRetriever;
        private readonly IFacialFeatureRetriever mouthRetriever;

        public BodyShapeLoader(IRetrieverFactory retrieverFactory, WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth)
        {
            Assert.IsNotNull(bodyshape);
            Assert.IsNotNull(eyes);
            Assert.IsNotNull(eyebrows);
            Assert.IsNotNull(mouth);
            Assert.AreEqual(WearableLiterals.Categories.BODY_SHAPE, bodyshape.data.category);
            Assert.AreEqual(WearableLiterals.Categories.EYES, eyes.data.category);
            Assert.AreEqual(WearableLiterals.Categories.EYEBROWS, eyebrows.data.category);
            Assert.AreEqual(WearableLiterals.Categories.MOUTH, mouth.data.category);

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
            if (status == IWearableLoader.Status.Succeeded)
            {
                PrepareMaterials(avatarSettings);
                return;
            }

            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            status = IWearableLoader.Status.Idle;

            await LoadWearable(container, ct);
            if (rendereable == null)
            {
                status = IWearableLoader.Status.Failed;
                return;
            }

            (headRenderer, upperBodyRenderer, lowerBodyRenderer, feetRenderer, eyesRenderer, eyebrowsRenderer, mouthRenderer) = AvatarSystemUtils.ExtractBodyshapeParts(bodyshapeRetriever.rendereable);

            (string eyesMainTextureUrl, string eyesMaskTextureUrl) = AvatarSystemUtils.GetFacialFeatureTexturesUrls(wearable.id, eyes);
            (string eyebrowsMainTextureUrl, string eyebrowsMaskTextureUrl) = AvatarSystemUtils.GetFacialFeatureTexturesUrls(wearable.id, eyebrows);
            (string mouthMainTextureUrl, string mouthMaskTextureUrl) = AvatarSystemUtils.GetFacialFeatureTexturesUrls(wearable.id, mouth);

            UniTask<(Texture main, Texture mask)> eyesTask = eyesRetriever.Retrieve(eyesMainTextureUrl, eyesMaskTextureUrl, ct);
            UniTask<(Texture main, Texture mask)> eyebrowsTask = eyebrowsRetriever.Retrieve(eyebrowsMainTextureUrl, eyebrowsMaskTextureUrl, ct);
            UniTask<(Texture main, Texture mask)> mouthTask = mouthRetriever.Retrieve(mouthMainTextureUrl, mouthMaskTextureUrl, ct);

            var (eyesResult, eyebrowsResult, mouthResult) = await (eyesTask, eyebrowsTask, mouthTask);
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            eyesRenderer.material = new Material(Resources.Load<Material>("Eye Material"));
            eyesRenderer.material.SetTexture(AvatarSystemUtils._EyesTexture, eyesResult.main);
            if (eyesResult.mask != null)
                eyesRenderer.material.SetTexture(AvatarSystemUtils._IrisMask, eyesResult.mask);

            eyebrowsRenderer.material = new Material(Resources.Load<Material>("Eyebrow Material"));
            eyebrowsRenderer.material.SetTexture(AvatarSystemUtils._BaseMap, eyebrowsResult.main);
            if (eyebrowsResult.mask != null)
                eyebrowsRenderer.material.SetTexture(AvatarSystemUtils._BaseMap, eyebrowsResult.mask);

            mouthRenderer.material = new Material(Resources.Load<Material>("Mouth Material"));
            mouthRenderer.material.SetTexture(AvatarSystemUtils._BaseMap, mouthResult.main);
            if (mouthResult.mask != null)
                mouthRenderer.material.SetTexture(AvatarSystemUtils._TintMask, mouthResult.mask);

            PrepareMaterials(avatarSettings);
            status = IWearableLoader.Status.Succeeded;
        }

        private void PrepareMaterials(AvatarSettings avatarSettings)
        {
            AvatarSystemUtils.PrepareMaterialColors(rendereable, avatarSettings.skinColor, avatarSettings.hairColor);
            eyesRenderer?.material.SetColor(AvatarSystemUtils._EyeTint, avatarSettings.eyesColor);
            eyebrowsRenderer?.material.SetColor(AvatarSystemUtils._BaseColor, avatarSettings.hairColor);
            mouthRenderer?.material.SetColor(AvatarSystemUtils._BaseColor, avatarSettings.skinColor);
        }

        private async UniTask<Rendereable> LoadWearable(GameObject container, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return null;
            }

            bodyshapeRetriever.Dispose();

            WearableItem.Representation representation = wearable.GetRepresentation(wearable.id);
            if (representation == null)
            {
                status = IWearableLoader.Status.Failed;
                return null;
            }

            Rendereable resultRendereable = await bodyshapeRetriever.Retrieve(container, wearable.GetContentProvider(wearable.id), wearable.baseUrlBundles, representation.mainFile, ct);
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return null;
            }

            return resultRendereable;
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