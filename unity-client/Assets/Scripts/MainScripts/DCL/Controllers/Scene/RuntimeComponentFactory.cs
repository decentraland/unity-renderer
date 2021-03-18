using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public interface IRuntimeComponentFactory
    {
        IComponent CreateComponent(int classId);
        void Initialize();
    }

    public class RuntimeComponentFactory : IRuntimeComponentFactory
    {
        private delegate IComponent ComponentBuilder(int classId);

        private Dictionary<int, ComponentBuilder> builders = new Dictionary<int, ComponentBuilder>();

        public IPoolableComponentFactory poolableComponentFactory { get; private set; }

        public void Initialize()
        {
            CoroutineStarter.Start(InitializeCoroutine());
        }

        IEnumerator InitializeCoroutine()
        {
            yield return null;
            poolableComponentFactory.PrewarmPools();
        }

        public RuntimeComponentFactory(IPoolableComponentFactory poolableComponentFactory = null)
        {
            this.poolableComponentFactory = poolableComponentFactory ?? PoolableComponentFactory.Create();

            // Transform
            builders.Add((int) CLASS_ID_COMPONENT.TRANSFORM, BuildComponent<DCLTransform>);

            // Shapes
            builders.Add((int) CLASS_ID.BOX_SHAPE, BuildComponent<BoxShape>);
            builders.Add((int) CLASS_ID.SPHERE_SHAPE, BuildComponent<SphereShape>);
            builders.Add((int) CLASS_ID.CYLINDER_SHAPE, BuildComponent<CylinderShape>);
            builders.Add((int) CLASS_ID.CONE_SHAPE, BuildComponent<ConeShape>);
            builders.Add((int) CLASS_ID.PLANE_SHAPE, BuildComponent<PlaneShape>);
            builders.Add((int) CLASS_ID.GLTF_SHAPE, BuildComponent<GLTFShape>);
            builders.Add((int) CLASS_ID.NFT_SHAPE, BuildComponent<NFTShape>);
            builders.Add((int) CLASS_ID.OBJ_SHAPE, BuildComponent<OBJShape>);
            builders.Add((int) CLASS_ID_COMPONENT.TEXT_SHAPE, BuildPoolableComponent);

            builders.Add((int) CLASS_ID_COMPONENT.BILLBOARD, BuildPoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.SMART_ITEM, BuildPoolableComponent);

            // Materials
            builders.Add((int) CLASS_ID.BASIC_MATERIAL, BuildComponent<BasicMaterial>);
            builders.Add((int) CLASS_ID.PBR_MATERIAL, BuildComponent<PBRMaterial>);
            builders.Add((int) CLASS_ID.TEXTURE, BuildComponent<DCLTexture>);

            // Audio
            builders.Add((int) CLASS_ID.AUDIO_CLIP, BuildComponent<DCLAudioClip>);
            builders.Add((int) CLASS_ID_COMPONENT.AUDIO_SOURCE, BuildPoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.AUDIO_STREAM, BuildPoolableComponent);

            // UI
            builders.Add((int) CLASS_ID.UI_INPUT_TEXT_SHAPE, BuildComponent<UIInputText>);
            builders.Add((int) CLASS_ID.UI_FULLSCREEN_SHAPE, BuildComponent<UIScreenSpace>);
            builders.Add((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE, BuildComponent<UIScreenSpace>);
            builders.Add((int) CLASS_ID.UI_CONTAINER_RECT, BuildComponent<UIContainerRect>);
            builders.Add((int) CLASS_ID.UI_SLIDER_SHAPE, BuildComponent<UIScrollRect>);
            builders.Add((int) CLASS_ID.UI_CONTAINER_STACK, BuildComponent<UIContainerStack>);
            builders.Add((int) CLASS_ID.UI_IMAGE_SHAPE, BuildComponent<UIImage>);
            builders.Add((int) CLASS_ID.UI_TEXT_SHAPE, BuildComponent<UIText>);
            builders.Add((int) CLASS_ID.FONT, BuildComponent<DCLFont>);

            // Video
            builders.Add((int) CLASS_ID.VIDEO_CLIP, BuildComponent<DCLVideoClip>);
            builders.Add((int) CLASS_ID.VIDEO_TEXTURE, BuildComponent<DCLVideoTexture>);

            // Builder in world
            builders.Add((int) CLASS_ID.NAME, BuildComponent<DCLName>);
            builders.Add((int) CLASS_ID.LOCKED_ON_EDIT, BuildComponent<DCLLockedOnEdit>);
            builders.Add((int) CLASS_ID.VISIBLE_ON_EDIT, BuildComponent<DCLVisibleOnEdit>);

            // Events
            builders.Add((int) CLASS_ID_COMPONENT.UUID_ON_UP, BuildUUIDComponent<OnPointerUp>);
            builders.Add((int) CLASS_ID_COMPONENT.UUID_ON_DOWN, BuildUUIDComponent<OnPointerDown>);
            builders.Add((int) CLASS_ID_COMPONENT.UUID_ON_CLICK, BuildUUIDComponent<OnClick>);

            // Others
            builders.Add((int) CLASS_ID_COMPONENT.AVATAR_SHAPE, BuildPoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.ANIMATOR, BuildPoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.GIZMOS, BuildPoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA, BuildPoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION, BuildPoolableComponent);
        }

        private IComponent BuildPoolableComponent(int classId)
        {
            return poolableComponentFactory.CreateItemFromId<BaseComponent>((CLASS_ID_COMPONENT) classId);
        }

        private T BuildComponent<T>(int classId)
            where T : IComponent, new()
        {
            return new T();
        }

        private T BuildUUIDComponent<T>(int classId)
            where T : UnityEngine.Component
        {
            var go = new GameObject("UUID Component");
            T newComponent = go.GetOrCreateComponent<T>();
            return newComponent;
        }

        public IComponent CreateComponent(int classId)
        {
            if (!builders.ContainsKey(classId))
            {
                Debug.LogError($"Unknown classId");
                return null;
            }

            IComponent newComponent = builders[classId](classId);

            return newComponent;
        }
    }
}