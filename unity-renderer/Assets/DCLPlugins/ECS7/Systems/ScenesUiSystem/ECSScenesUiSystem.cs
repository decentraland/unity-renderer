using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine.UIElements;

namespace ECSSystems.ScenesUiSystem
{
    public class ECSScenesUiSystem : IDisposable
    {
        private class State
        {
            public UIDocument uiDocument;
            public IInternalECSComponent<InternalUiContainer> internalUiContainerComponent;
            public IWorldState worldState;
            public BaseList<IParcelScene> loadedScenes;
            public string lastSceneId;
            public bool isPendingSceneUI;
            public IParcelScene currentScene;
        }

        private readonly State state;

        public ECSScenesUiSystem(UIDocument uiDocument,
            IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            BaseList<IParcelScene> loadedScenes,
            IWorldState worldState)
        {
            state = new State()
            {
                uiDocument = uiDocument,
                internalUiContainerComponent = internalUiContainerComponent,
                worldState = worldState,
                loadedScenes = loadedScenes,
                lastSceneId = null,
                isPendingSceneUI = false,
                currentScene = null
            };

            state.loadedScenes.OnRemoved += LoadedScenesOnOnRemoved;
        }

        public void Dispose()
        {
            state.loadedScenes.OnRemoved -= LoadedScenesOnOnRemoved;
        }

        public void Update()
        {
            string currentSceneId = state.worldState.GetCurrentSceneId();
            bool sceneChanged = state.lastSceneId != currentSceneId;
            state.lastSceneId = currentSceneId;

            ApplyParenting(state.internalUiContainerComponent);

            // clear UI if scene changed
            if (sceneChanged)
            {
                ClearUI(state.uiDocument);
                state.isPendingSceneUI = !string.IsNullOrEmpty(currentSceneId);
                state.currentScene = null;
            }

            // UI not set for current scene yet
            if (state.isPendingSceneUI)
            {
                // we get current scene reference
                state.currentScene ??= GetCurrentScene(currentSceneId, state.loadedScenes);

                // we apply current scene UI
                if (state.currentScene != null)
                {
                    if (ApplySceneUI(state.internalUiContainerComponent, state.uiDocument, state.currentScene))
                    {
                        state.isPendingSceneUI = false;
                    }
                }
            }
        }

        private void LoadedScenesOnOnRemoved(IParcelScene scene)
        {
            if (scene.sceneData.id == state.lastSceneId)
            {
                state.lastSceneId = null;
            }
        }

        internal static void ApplyParenting(IInternalECSComponent<InternalUiContainer> internalUiContainerComponent)
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

                InternalUiContainer parentDataModel = GetParentContainerModel(
                    internalUiContainerComponent,
                    uiContainerData.scene,
                    uiContainerData.model.parentId);

                // apply parenting
                if (parentDataModel != null)
                {
                    var currentContainerModel = uiContainerData.model;
                    parentDataModel.rootElement.Add(uiContainerData.model.rootElement);
                    currentContainerModel.parentElement = parentDataModel.rootElement;

                    internalUiContainerComponent.PutFor(uiContainerData.scene, uiContainerData.model.parentId, parentDataModel);
                    internalUiContainerComponent.PutFor(uiContainerData.scene, uiContainerData.entity, currentContainerModel);
                }
            }
        }

        internal static void ClearUI(UIDocument uiDocument)
        {
            if (uiDocument.rootVisualElement.childCount > 0)
            {
                uiDocument.rootVisualElement.Clear();
            }
        }

        internal static IParcelScene GetCurrentScene(string sceneId, IList<IParcelScene> loadedScenes)
        {
            if (string.IsNullOrEmpty(sceneId))
                return null;

            IParcelScene currentScene = null;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if (loadedScenes[i].sceneData.id == sceneId)
                {
                    currentScene = loadedScenes[i];
                    break;
                }
            }

            return currentScene;
        }

        internal static bool ApplySceneUI(IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            UIDocument uiDocument, IParcelScene currentScene)
        {
            IECSReadOnlyComponentData<InternalUiContainer> sceneRootUiContainer =
                internalUiContainerComponent.GetFor(currentScene, SpecialEntityId.SCENE_ROOT_ENTITY);

            if (sceneRootUiContainer != null)
            {
                var model = sceneRootUiContainer.model;
                uiDocument.rootVisualElement.Add(model.rootElement);
                model.parentElement = uiDocument.rootVisualElement;
                internalUiContainerComponent.PutFor(currentScene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
                return true;
            }
            return false;
        }

        private static InternalUiContainer GetParentContainerModel(IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            IParcelScene scene, long parentId)
        {
            InternalUiContainer parentDataModel =
                internalUiContainerComponent.GetFor(scene, parentId)?.model;

            // create root entity ui container if needed
            if (parentDataModel == null && parentId == SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                parentDataModel = new InternalUiContainer();
            }
            return parentDataModel;
        }
    }
}