using System;
using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using UnityEngine;

namespace ECSSystems.VisibilitySystem
{
    public static class ECSVisibilitySystem
    {
        private class State
        {
            public IECSReadOnlyComponentsGroup<InternalRenderers, InternalVisibility> componentsGroup;
            public IInternalECSComponent<InternalRenderers> renderersComponent;
            public IInternalECSComponent<InternalVisibility> visibilityComponent;

        }

        public static Action CreateSystem(IECSReadOnlyComponentsGroup<InternalRenderers, InternalVisibility> componentsGroup,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            IInternalECSComponent<InternalVisibility> visibilityComponent)
        {
            var state = new State()
            {
                componentsGroup = componentsGroup,
                renderersComponent = renderersComponent,
                visibilityComponent = visibilityComponent
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            var componentGroup = state.componentsGroup.group;

            for (int i = 0; i < componentGroup.Count; i++)
            {
                var entityData = componentGroup[i];
                InternalRenderers renderersModel = entityData.componentData1.model;
                InternalVisibility visibilityModel = entityData.componentData2.model;


                // if neither component has changed then we skip this entity
                if (!renderersModel.dirty && !visibilityModel.dirty)
                    continue;

                IList<Renderer> renderers = renderersModel.renderers;

                for (int j = 0; j < renderers.Count; j++)
                {
                    Renderer renderer = renderers[j];
                    renderer.enabled = visibilityModel.visible;
                }
            }
        }
    }
}