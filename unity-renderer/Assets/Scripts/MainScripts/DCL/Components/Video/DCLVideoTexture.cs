using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;
using DCL.Components.Video.Plugin;

namespace DCL.Components
{
    public class DCLVideoTexture : DCLTexture
    {
#if UNITY_EDITOR
        internal static bool isTest = true;
#else
        internal static bool isTest = false;
#endif

        [System.Serializable]
        new public class Model
        {
            public string videoClipId;
            public bool playing = false;
            public float volume = 1f;
            public float playbackRate = 1f;
            public bool loop = false;
            public float seek = -1;
            public BabylonWrapMode wrap = BabylonWrapMode.CLAMP;
            public FilterMode samplingMode = FilterMode.Bilinear;
        }

        new protected Model model;

        private WebVideoPlayer texturePlayer;
        private Coroutine texturePlayerUpdateRoutine;
        private float baseVolume;
        private float distanceVolumeModifier = 1f;
        private bool isPlayStateDirty = false;
        internal bool isVisible = false;

        internal Dictionary<string, MaterialInfo> attachedMaterials = new Dictionary<string, MaterialInfo>();

        public DCLVideoTexture(ParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

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

            if (texturePlayer == null)
            {
                DCLVideoClip dclVideoClip = scene.GetSharedComponent(model.videoClipId) as DCLVideoClip;
                if (dclVideoClip == null)
                {
                    Debug.LogError("Wrong video clip type when playing VideoTexture!!");
                    yield break;
                }
                texturePlayer = new WebVideoPlayer(id, dclVideoClip.GetUrl(), dclVideoClip.isStream);
                texturePlayerUpdateRoutine = CoroutineStarter.Start(VideoTextureUpdate());
                CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChanged;
                scene.OnEntityRemoved += OnEntityRemoved;
                Settings.i.OnGeneralSettingsChanged += OnSettingsChanged;
            }

            // NOTE: create texture for testing cause real texture will only be created on web platform
            if (isTest)
            {
                if (texture == null)
                    texture = new Texture2D(1, 1);
            }

            if (texture == null)
            {
                while (texturePlayer.texture == null && !texturePlayer.isError)
                {
                    yield return null;
                }

                if (texturePlayer.isError)
                {
                    if (texturePlayerUpdateRoutine != null)
                    {
                        CoroutineStarter.Stop(texturePlayerUpdateRoutine);
                        texturePlayerUpdateRoutine = null;
                    }
                    yield break;
                }
                texture = texturePlayer.texture;
                isPlayStateDirty = true;
            }

            if (texturePlayer != null)
            {
                if (texturePlayer.playing && !model.playing) texturePlayer.Pause();
                else if (model.playing) texturePlayer.Play();

                if (baseVolume != model.volume)
                {
                    baseVolume = model.volume;
                    UpdateVolume();
                }

                if (model.seek >= 0)
                {
                    texturePlayer.SetTime(model.seek);
                    model.seek = -1;
                }
                texturePlayer.SetPlaybackRate(model.playbackRate);
                texturePlayer.SetLoop(model.loop);
            }
        }

        private bool HasTexturePropertiesChanged()
        {
            return texture.wrapMode != unityWrap || texture.filterMode != unitySamplingMode;
        }

        private void ApplyTextureProperties()
        {
            texture.wrapMode = unityWrap;
            texture.filterMode = unitySamplingMode;
            texture.Compress(false);
            texture.Apply(unitySamplingMode != FilterMode.Point, true);
        }

        private IEnumerator VideoTextureUpdate()
        {
            while (true)
            {
                if (isPlayStateDirty)
                {
                    CalculateVideoVolumeAndPlayStatus();
                    isPlayStateDirty = false;
                }
                if (texturePlayer != null && !isTest)
                {
                    texturePlayer.UpdateWebVideoTexture();
                }
                yield return null;
            }
        }

        private void CalculateVideoVolumeAndPlayStatus()
        {
            isVisible = false;
            float minDistance = float.MaxValue;
            distanceVolumeModifier = 0;

            if (attachedMaterials.Count > 0)
            {
                using (var iterator = attachedMaterials.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        var materialInfo = iterator.Current;
                        if (materialInfo.Value.IsVisible())
                        {
                            isVisible = true;
                            var entityDist = materialInfo.Value.GetClosestDistanceSqr(DCLCharacterController.i.transform.position);
                            if (entityDist < minDistance) minDistance = entityDist;
                            // NOTE: if current minDistance is enough for full volume then there is no need to keep iterating to check distances
                            if (minDistance <= DCL.Configuration.ParcelSettings.PARCEL_SIZE * DCL.Configuration.ParcelSettings.PARCEL_SIZE) break;
                        }
                    }
                }
            }

            if (isVisible)
            {
                const float maxDistanceBlockForSound = 6;
                float sqrParcelDistance = DCL.Configuration.ParcelSettings.PARCEL_SIZE * DCL.Configuration.ParcelSettings.PARCEL_SIZE * 2.25f;
                distanceVolumeModifier = 1 - Mathf.Clamp01(Mathf.FloorToInt(minDistance / sqrParcelDistance) / maxDistanceBlockForSound);
            }

            if (texturePlayer != null)
            {
                texturePlayer.visible = isVisible;
            }

            UpdateVolume();
        }

        private void UpdateVolume()
        {
            if (texturePlayer != null)
            {
                texturePlayer.SetVolume(baseVolume * distanceVolumeModifier * Settings.i.generalSettings.sfxVolume);
            }
        }

        private void OnPlayerCoordsChanged(Vector2Int coords, Vector2Int prevCoords)
        {
            isPlayStateDirty = true;
        }

        public override void AttachTo(PBRMaterial material)
        {
            base.AttachTo(material);
            AttachToMaterial(material);
        }

        public override void DetachFrom(PBRMaterial material)
        {
            base.DetachFrom(material);
            DetachFromMaterial(material);
        }

        public override void AttachTo(BasicMaterial material)
        {
            base.AttachTo(material);
            AttachToMaterial(material);
        }

        public override void DetachFrom(BasicMaterial material)
        {
            base.DetachFrom(material);
            DetachFromMaterial(material);
        }

        private void AttachToMaterial(BaseDisposable baseDisposable)
        {
            if (!attachedMaterials.ContainsKey(baseDisposable.id))
            {
                attachedMaterials.Add(baseDisposable.id, new MaterialComponent(baseDisposable));
                baseDisposable.OnAttach += OnEntityAttachedMaterial;
                baseDisposable.OnDetach += OnEntityDetachedMaterial;
                isPlayStateDirty = true;

                if (baseDisposable.attachedEntities.Count > 0)
                {
                    using (var iterator = baseDisposable.attachedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            var entity = iterator.Current;
                            entity.OnShapeUpdated -= OnEntityShapeUpdated;
                            entity.OnShapeUpdated += OnEntityShapeUpdated;
                        }
                    }
                }
            }
        }

        private void DetachFromMaterial(BaseDisposable baseDisposable)
        {
            if (attachedMaterials.ContainsKey(baseDisposable.id))
            {
                attachedMaterials.Remove(baseDisposable.id);
                baseDisposable.OnAttach -= OnEntityAttachedMaterial;
                baseDisposable.OnDetach -= OnEntityDetachedMaterial;
                isPlayStateDirty = true;
            }
        }

        // TODO: we will need an event for visibility change on UI for supporting video
        public override void AttachTo(UIImage image)
        {
            if (!attachedMaterials.ContainsKey(image.id))
            {
                attachedMaterials.Add(image.id, new UIShapeComponent(image));
                isPlayStateDirty = true;
            }
        }

        public override void DetachFrom(UIImage image)
        {
            if (attachedMaterials.ContainsKey(image.id))
            {
                attachedMaterials.Remove(image.id);
                isPlayStateDirty = true;
            }
        }

        void OnEntityRemoved(DecentralandEntity entity)
        {
            isPlayStateDirty = true;
        }

        void OnSettingsChanged(SettingsData.GeneralSettings settings)
        {
            UpdateVolume();
        }

        public override void Dispose()
        {
            Settings.i.OnGeneralSettingsChanged -= OnSettingsChanged;
            CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChanged;
            if (scene != null) scene.OnEntityRemoved -= OnEntityRemoved;
            if (texturePlayerUpdateRoutine != null)
            {
                CoroutineStarter.Stop(texturePlayerUpdateRoutine);
                texturePlayerUpdateRoutine = null;
            }

            if (texturePlayer != null)
            {
                texturePlayer.Dispose();
                texturePlayer = null;
            }

            if (isTest && texture != null)
            {
                UnityEngine.Object.Destroy(texture);
            }
            texture = null;
            base.Dispose();
        }

        private void OnEntityAttachedMaterial(DecentralandEntity entity)
        {
            entity.OnShapeUpdated += OnEntityShapeUpdated;
        }

        private void OnEntityDetachedMaterial(DecentralandEntity entity)
        {
            entity.OnShapeUpdated -= OnEntityShapeUpdated;
        }

        private void OnEntityShapeUpdated(DecentralandEntity entity)
        {
            isPlayStateDirty = true;
        }

        internal interface MaterialInfo
        {
            float GetClosestDistanceSqr(Vector3 fromPosition);
            bool IsVisible();
        }

        struct MaterialComponent : MaterialInfo
        {
            BaseDisposable component;

            float MaterialInfo.GetClosestDistanceSqr(Vector3 fromPosition)
            {
                float dist = int.MaxValue;
                if (component.attachedEntities.Count > 0)
                {
                    using (var iterator = component.attachedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            var entity = iterator.Current;
                            if (IsEntityVisible(entity))
                            {
                                var entityDist = (entity.meshRootGameObject.transform.position - fromPosition).sqrMagnitude;
                                if (entityDist < dist) dist = entityDist;
                            }
                        }
                    }
                }
                return dist;
            }

            bool MaterialInfo.IsVisible()
            {
                if (component.attachedEntities.Count > 0)
                {
                    using (var iterator = component.attachedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            if (IsEntityVisible(iterator.Current))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            bool IsEntityVisible(DecentralandEntity entity)
            {
                if (entity.meshesInfo == null) return false;
                if (entity.meshesInfo.currentShape == null) return false;
                return entity.meshesInfo.currentShape.IsVisible();
            }

            public MaterialComponent(BaseDisposable component)
            {
                this.component = component;
            }
        }

        struct UIShapeComponent : MaterialInfo
        {
            UIShape shape;

            float MaterialInfo.GetClosestDistanceSqr(Vector3 fromPosition)
            {
                return 0;
            }

            bool MaterialInfo.IsVisible()
            {
                if (!shape.model.visible) return false;
                return IsParentVisible(shape);
            }

            bool IsParentVisible(UIShape shape)
            {
                UIShape parent = shape.parentUIComponent;
                if (parent == null) return true;
                if (parent.referencesContainer.canvasGroup.alpha == 0)
                {
                    return false;
                }
                return IsParentVisible(parent);
            }

            public UIShapeComponent(UIShape image)
            {
                shape = image;
            }
        }
    }
}