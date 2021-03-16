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
        IComponent CreateComponent(int classId, object model);
        void Initialize();
    }

    public class RuntimeComponentFactory : IRuntimeComponentFactory
    {
        private delegate IComponent ComponentBuilder(int classId, object model);

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

            // Shapes
            builders.Add((int) CLASS_ID.BOX_SHAPE, CreateComponent<BoxShape>);
            builders.Add((int) CLASS_ID.SPHERE_SHAPE, CreateComponent<SphereShape>);
            builders.Add((int) CLASS_ID.CYLINDER_SHAPE, CreateComponent<CylinderShape>);
            builders.Add((int) CLASS_ID.CONE_SHAPE, CreateComponent<ConeShape>);
            builders.Add((int) CLASS_ID.PLANE_SHAPE, CreateComponent<PlaneShape>);
            builders.Add((int) CLASS_ID.GLTF_SHAPE, CreateComponent<GLTFShape>);
            builders.Add((int) CLASS_ID.NFT_SHAPE, CreateComponent<NFTShape>);
            builders.Add((int) CLASS_ID.OBJ_SHAPE, CreateComponent<OBJShape>);
            builders.Add((int) CLASS_ID_COMPONENT.TEXT_SHAPE, CreatePoolableComponent);

            builders.Add((int) CLASS_ID_COMPONENT.BILLBOARD, CreatePoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.SMART_ITEM, CreatePoolableComponent);

            // Materials
            builders.Add((int) CLASS_ID.BASIC_MATERIAL, CreateComponent<BasicMaterial>);
            builders.Add((int) CLASS_ID.PBR_MATERIAL, CreateComponent<PBRMaterial>);
            builders.Add((int) CLASS_ID.TEXTURE, CreateComponent<DCLTexture>);

            // Audio
            builders.Add((int) CLASS_ID.AUDIO_CLIP, CreateComponent<DCLAudioClip>);
            builders.Add((int) CLASS_ID_COMPONENT.AUDIO_SOURCE, CreatePoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.AUDIO_STREAM, CreatePoolableComponent);

            // UI
            builders.Add((int) CLASS_ID.UI_INPUT_TEXT_SHAPE, CreateComponent<UIInputText>);
            builders.Add((int) CLASS_ID.UI_FULLSCREEN_SHAPE, CreateComponent<UIScreenSpace>);
            builders.Add((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE, CreateComponent<UIScreenSpace>);
            builders.Add((int) CLASS_ID.UI_CONTAINER_RECT, CreateComponent<UIContainerRect>);
            builders.Add((int) CLASS_ID.UI_SLIDER_SHAPE, CreateComponent<UIScrollRect>);
            builders.Add((int) CLASS_ID.UI_CONTAINER_STACK, CreateComponent<UIContainerStack>);
            builders.Add((int) CLASS_ID.UI_IMAGE_SHAPE, CreateComponent<UIImage>);
            builders.Add((int) CLASS_ID.UI_TEXT_SHAPE, CreateComponent<UIText>);
            builders.Add((int) CLASS_ID.FONT, CreateComponent<DCLFont>);

            // Video
            builders.Add((int) CLASS_ID.VIDEO_CLIP, CreateComponent<DCLVideoClip>);
            builders.Add((int) CLASS_ID.VIDEO_TEXTURE, CreateComponent<DCLVideoTexture>);

            // Builder in world
            builders.Add((int) CLASS_ID.NAME, CreateComponent<DCLName>);
            builders.Add((int) CLASS_ID.LOCKED_ON_EDIT, CreateComponent<DCLLockedOnEdit>);
            builders.Add((int) CLASS_ID.VISIBLE_ON_EDIT, CreateComponent<DCLVisibleOnEdit>);

            // Events
            builders.Add((int) CLASS_ID_COMPONENT.UUID_CALLBACK, CreateUUIDComponent);

            // Others
            builders.Add((int) CLASS_ID_COMPONENT.AVATAR_SHAPE, CreatePoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.ANIMATOR, CreatePoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.GIZMOS, CreatePoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA, CreatePoolableComponent);
            builders.Add((int) CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION, CreatePoolableComponent);
        }

        private IComponent CreatePoolableComponent(int classId, object model)
        {
            return poolableComponentFactory.CreateItemFromId<BaseComponent>((CLASS_ID_COMPONENT) classId);
        }

        private T CreateComponent<T>(int classId, object model)
            where T : IComponent, new()
        {
            return new T();
        }

        private UUIDComponent CreateUUIDComponent(int classId, object model)
        {
            if (!(model is OnPointerEvent.Model uuidModel))
            {
                Debug.LogError("Data is not a DCLTransform.Model type!");
                return null;
            }

            var go = new GameObject("UUID Component");

            UUIDComponent newComponent = null;

            switch (uuidModel.type)
            {
                case OnClick.NAME:
                    newComponent = go.GetOrCreateComponent<OnClick>();
                    break;
                case OnPointerDown.NAME:
                    newComponent = go.GetOrCreateComponent<OnPointerDown>();
                    break;
                case OnPointerUp.NAME:
                    newComponent = go.GetOrCreateComponent<OnPointerUp>();
                    break;
            }

            return newComponent;
        }

        public IComponent CreateComponent(int classId, object model)
        {
            if (!builders.ContainsKey(classId))
            {
                Debug.LogError($"Unknown classId");
                return null;
            }

            IComponent newComponent = builders[classId](classId, model);

            return newComponent;
        }
    }
}