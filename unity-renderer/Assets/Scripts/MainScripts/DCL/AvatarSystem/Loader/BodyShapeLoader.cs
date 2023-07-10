using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using UnityEngine;
using DCL.Shaders;
using Object = UnityEngine.Object;

namespace AvatarSystem
{
    public class BodyShapeLoader : IBodyshapeLoader
    {
        public WearableItem bodyShape { get; }
        public Rendereable rendereable => bodyShapeRetriever?.rendereable;
        public IWearableLoader.Status status { get; private set; }
        public WearableItem eyes { get; }
        public WearableItem eyebrows { get; }
        public WearableItem mouth { get; }
        public SkinnedMeshRenderer eyesRenderer { get; internal set; }
        public SkinnedMeshRenderer eyebrowsRenderer { get; internal set; }
        public SkinnedMeshRenderer mouthRenderer { get; internal set; }
        public SkinnedMeshRenderer headRenderer { get; private set; }
        public SkinnedMeshRenderer feetRenderer { get; private set; }
        public SkinnedMeshRenderer handsRenderer { get; private set; }
        public SkinnedMeshRenderer upperBodyRenderer { get; private set; }
        public SkinnedMeshRenderer lowerBodyRenderer { get; private set; }

        // This list contains uncategorized body parts that also have their own renderers
        // this enables us to modify the body shape with new parts without ignoring them during the mesh combine process
        public IList<SkinnedMeshRenderer> extraRenderers { get; set; }

        internal readonly IWearableRetriever bodyShapeRetriever;
        internal readonly IFacialFeatureRetriever eyesRetriever;
        internal readonly IFacialFeatureRetriever eyebrowsRetriever;
        internal readonly IFacialFeatureRetriever mouthRetriever;

        private readonly Dictionary<SkinnedMeshRenderer, (Transform rootBone, Transform[] bones)> originalBones = new ();

        public BodyShapeLoader(IRetrieverFactory retrieverFactory, BodyWearables bodyWearables)
        {
            this.bodyShape = bodyWearables.BodyShape;
            this.eyes = bodyWearables.Eyes;
            this.eyebrows = bodyWearables.Eyebrows;
            this.mouth = bodyWearables.Mouth;

            bodyShapeRetriever = retrieverFactory.GetWearableRetriever();
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

                // Store the original bones.
                originalBones.Clear();

                foreach (SkinnedMeshRenderer skm in bodyShapeRetriever.rendereable.renderers.OfType<SkinnedMeshRenderer>()) { originalBones[skm] = (skm.rootBone, skm.bones); }

                (headRenderer, upperBodyRenderer, lowerBodyRenderer, feetRenderer, eyesRenderer, eyebrowsRenderer, mouthRenderer, handsRenderer, extraRenderers) = AvatarSystemUtils.ExtractBodyShapeParts(bodyShapeRetriever.rendereable);

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

        public void SetBones(Transform rootBone, Transform[] bones)
        {
            AvatarSystemUtils.CopyBones(rootBone, bones, rendereable.renderers.OfType<SkinnedMeshRenderer>());
        }

        private async UniTask LoadMouth(CancellationToken ct)
        {
            if (mouth == null) return;
            (Texture main, Texture mask) = await mouthRetriever.Retrieve(mouth, bodyShape.id, ct);
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
            (Texture main, Texture mask) = await eyebrowsRetriever.Retrieve(eyebrows, bodyShape.id, ct);
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
            (Texture main, Texture mask) = await eyesRetriever.Retrieve(eyes, bodyShape.id, ct);
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

            bodyShapeRetriever.Dispose();

            Rendereable bodyshapeRenderable = await bodyShapeRetriever.Retrieve(container, bodyShape, bodyShape.id, ct);

            if (bodyshapeRenderable == null) // fail safe, we shouldnt reach this since .Retrieve should throw if anything goes wrong
                throw new Exception("Couldn't load bodyshape");

            return bodyshapeRenderable;
        }

        public bool IsValid(BodyWearables bodyWearables)
        {
            if (bodyShape.id != bodyWearables.BodyShape?.id) return false;
            if (this.eyebrows?.id != bodyWearables.Eyebrows?.id) return false;
            if (this.mouth?.id != bodyWearables.Mouth?.id) return false;
            if (this.eyes?.id != bodyWearables.Eyes?.id) return false;
            return true;
        }

        public void DisableFacialRenderers()
        {
            if (eyesRenderer != null)
                eyesRenderer.enabled = false;

            if (eyebrowsRenderer != null)
                eyebrowsRenderer.enabled = false;

            if (mouthRenderer != null)
                mouthRenderer.enabled = false;
        }

        public void Dispose()
        {
            status = IWearableLoader.Status.Idle;

            //Restore bones
            foreach ((SkinnedMeshRenderer skm, (Transform rootBone, Transform[] bones)) in originalBones)
            {
                skm.rootBone = rootBone;
                skm.bones = bones;
            }

            originalBones.Clear();

            bodyShapeRetriever?.Dispose();
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
