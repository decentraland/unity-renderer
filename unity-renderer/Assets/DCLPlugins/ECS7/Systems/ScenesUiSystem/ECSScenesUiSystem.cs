using System;
using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine.UIElements;

namespace ECSSystems.ScenesUiSystem
{
    public static class ECSScenesUiSystem
    {
        private class State
        {
            public UIDocument uiDocument;
            public IInternalECSComponent<InternalUiContainer> internalUiContainerComponent;
            public IWorldState worldState;
            public BaseList<IParcelScene> loadedScenes;
            public string lastSceneId;
        }

        public static Action CreateSystem(UIDocument uiDocument,
            IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            BaseList<IParcelScene> loadedScenes,
            IWorldState worldState)
        {
            var state = new State()
            {
                uiDocument = uiDocument,
                internalUiContainerComponent = internalUiContainerComponent,
                worldState = worldState,
                loadedScenes = loadedScenes,
                lastSceneId = null
            };

            return () => Update(state);
        }

        private static void Update(State state)
        {
            string currentSceneId = state.worldState.GetCurrentSceneId();
            bool sceneChanged = state.lastSceneId != currentSceneId;
            state.lastSceneId = currentSceneId;

            ApplyParenting(state.internalUiContainerComponent);

            if (sceneChanged)
            {
                ApplySceneChange(state.internalUiContainerComponent, state.loadedScenes, state.uiDocument, currentSceneId);
            }
        }

        private static void ApplyParenting(IInternalECSComponent<InternalUiContainer> internalUiContainerComponent)
        {
            // check for orphan ui containers
            var allContainers = internalUiContainerComponent.GetForAll();
            for (int i = 0; i < allContainers.Count; i++)
            {
                var uiContainerData = allContainers[i].value;

                // skip ui containers on root entity since no parenting is needed
                if (uiContainerData.entity.entityId == SpecialEntityId.SCENE_ROOT_ENTITY)
                    continue;

                // skip containers with parent already set
                if (uiContainerData.model.parentElement != null)
                    continue;

                var parentData = internalUiContainerComponent.GetFor(uiContainerData.scene, uiContainerData.model.parentId);

                // create root entity ui container if needed
                if (parentData == null && uiContainerData.model.parentId == SpecialEntityId.SCENE_ROOT_ENTITY)
                {
                    internalUiContainerComponent.PutFor(uiContainerData.scene,
                        SpecialEntityId.SCENE_ROOT_ENTITY,
                        new InternalUiContainer()
                        {
                            hasTransform = true
                        });
                }
                // apply parenting
                else if (parentData != null)
                {
                    var parentContainerModel = parentData.model;
                    var currentContainerModel = uiContainerData.model;
                    parentContainerModel.rootElement.Add(uiContainerData.model.rootElement);
                    currentContainerModel.parentElement = parentData.model.rootElement;

                    internalUiContainerComponent.PutFor(parentData.scene, parentData.entity, parentContainerModel);
                    internalUiContainerComponent.PutFor(uiContainerData.scene, uiContainerData.entity, currentContainerModel);
                }
            }
        }

        private static void ApplySceneChange(IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            BaseList<IParcelScene> loadedScenes, UIDocument uiDocument, string sceneId)
        {
            if (uiDocument.rootVisualElement.childCount > 0)
            {
                uiDocument.rootVisualElement.Clear();
            }

            if (string.IsNullOrEmpty(sceneId))
                return;

            IParcelScene currentScene = null;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if (loadedScenes[i].sceneData.id == sceneId)
                {
                    currentScene = loadedScenes[i];
                    break;
                }
            }

            if (currentScene == null)
                return;

            IECSReadOnlyComponentData<InternalUiContainer> sceneRootUiContainer =
                internalUiContainerComponent.GetFor(currentScene, SpecialEntityId.SCENE_ROOT_ENTITY);

            if (sceneRootUiContainer != null)
            {
                uiDocument.rootVisualElement.Add(sceneRootUiContainer.model.rootElement);
            }
        }
    }
}