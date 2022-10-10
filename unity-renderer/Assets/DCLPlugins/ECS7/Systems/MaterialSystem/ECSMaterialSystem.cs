using System;
using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using UnityEngine;
using UnityEngine.Rendering;

namespace ECSSystems.MaterialSystem
{
    public static class ECSMaterialSystem
    {
        private class State
        {
            public IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> componentsGroup;
            public IInternalECSComponent<InternalTexturizable> texturizableComponent;
            public IInternalECSComponent<InternalMaterial> materialComponent;
        }

        public static Action CreateSystem(IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> componentsGroup,
            IInternalECSComponent<InternalTexturizable> texturizableComponent,
            IInternalECSComponent<InternalMaterial> materialComponent)
        {
            var state = new State()
            {
                componentsGroup = componentsGroup,
                texturizableComponent = texturizableComponent,
                materialComponent = materialComponent
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            var componentGroup = state.componentsGroup.group;

            for (int i = 0; i < componentGroup.Count; i++)
            {
                var entityData = componentGroup[i];
                InternalMaterial materialModel = entityData.componentData1.model;
                InternalTexturizable texturizableModel = entityData.componentData2.model;

                // if neither component has changed then we skip this entity
                if (!materialModel.dirty && !texturizableModel.dirty)
                    continue;

                IList<Renderer> renderers = texturizableModel.renderers;
                Material material = materialModel.material;

                for (int j = 0; j < renderers.Count; j++)
                {
                    Renderer renderer = renderers[j];
                    if (renderer.sharedMaterial != material)
                    {
                        renderer.sharedMaterial = material;
                    }
                    renderer.shadowCastingMode = materialModel.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                }
            }
        }
    }
}