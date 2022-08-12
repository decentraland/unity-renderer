using System;
using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using UnityEngine;

namespace ECSSystems.MaterialSystem
{
    public static class ECSMaterialSystem
    {
        private class State
        {
            public IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> componentsGroup;
            public IInternalECSComponents internalEcsComponents;
        }

        public static Action CreateSystem(IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> componentsGroup,
            IInternalECSComponents internalEcsComponents)
        {
            var state = new State()
            {
                componentsGroup = componentsGroup,
                internalEcsComponents = internalEcsComponents
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

                for (int j = 0; j < renderers.Count; i++)
                {
                    var renderer = renderers[i];
                    if (renderer.sharedMaterial != material)
                    {
                        renderer.sharedMaterial = material;
                    }
                }

                materialModel.dirty = false;
                texturizableModel.dirty = false;

                state.internalEcsComponents.materialComponent.PutFor(entityData.scene, entityData.entity, materialModel);
                state.internalEcsComponents.texturizableComponent.PutFor(entityData.scene, entityData.entity, texturizableModel);
            }
        }
    }
}