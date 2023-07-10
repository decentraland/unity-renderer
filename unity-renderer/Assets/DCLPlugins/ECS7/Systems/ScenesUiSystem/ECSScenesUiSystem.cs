using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ECSSystems.ScenesUiSystem
{
    public class ECSScenesUiSystem : IDisposable
    {
        private readonly UIDocument uiDocument;
        private readonly IInternalECSComponent<InternalUiContainer> internalUiContainerComponent;
        private readonly IWorldState worldState;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly BooleanVariable hideUiEventVariable;
        private readonly BaseVariable<bool> isSceneUIEnabled;

        private int lastSceneNumber;
        private bool isPendingSceneUI;
        private IParcelScene currentScene;
        private HashSet<IParcelScene> scenesUiToSort = new HashSet<IParcelScene>();

        public ECSScenesUiSystem(UIDocument uiDocument,
            IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            BaseList<IParcelScene> loadedScenes,
            IWorldState worldState,
            BooleanVariable hideUiEventVariable,
            BaseVariable<bool> isSceneUIEnabled)
        {
            this.uiDocument = uiDocument;
            this.internalUiContainerComponent = internalUiContainerComponent;
            this.worldState = worldState;
            this.loadedScenes = loadedScenes;
            this.hideUiEventVariable = hideUiEventVariable;
            this.isSceneUIEnabled = isSceneUIEnabled;

            lastSceneNumber = -1;
            isPendingSceneUI = true;
            currentScene = null;

            loadedScenes.OnRemoved += LoadedScenesOnOnRemoved;
            hideUiEventVariable.OnChange += OnHideAllUiEvent;
            isSceneUIEnabled.OnChange += OnHideSceneUiEvent;

            OnHideAllUiEvent(hideUiEventVariable.Get(), false);
        }

        public void Dispose()
        {
            loadedScenes.OnRemoved -= LoadedScenesOnOnRemoved;
            hideUiEventVariable.OnChange -= OnHideAllUiEvent;
            isSceneUIEnabled.OnChange -= OnHideSceneUiEvent;
        }

        public void Update()
        {
            int currentSceneNumber = worldState.GetCurrentSceneNumber();
            bool sceneChanged = lastSceneNumber != currentSceneNumber;
            lastSceneNumber = currentSceneNumber;

            ApplyParenting(ref scenesUiToSort, uiDocument, internalUiContainerComponent, currentSceneNumber);

            // If parenting detects that the order for ui elements has changed, it should sort the ui tree
            if (scenesUiToSort.Count > 0)
            {
                SortSceneUiTree(internalUiContainerComponent, scenesUiToSort);
            }

            // clear UI if scene changed
            if (sceneChanged && !isPendingSceneUI)
            {
                ClearCurrentSceneUI(uiDocument, currentScene, internalUiContainerComponent);
                isPendingSceneUI = currentSceneNumber > 0;
            }

            if (sceneChanged && currentScene != null && currentSceneNumber != currentScene.sceneData.sceneNumber)
            {
                currentScene = null;
            }

            // UI not set for current scene yet
            if (isPendingSceneUI)
            {
                // we get current scene reference
                currentScene ??= GetCurrentScene(currentSceneNumber, loadedScenes);

                // we apply current scene UI
                if (currentScene != null)
                {
                    if (ApplySceneUI(internalUiContainerComponent, uiDocument, currentScene, isSceneUIEnabled))
                    {
                        isPendingSceneUI = false;
                    }
                }
            }
        }

        private void LoadedScenesOnOnRemoved(IParcelScene scene)
        {
            if (scene.sceneData.sceneNumber == lastSceneNumber)
            {
                lastSceneNumber = -1;
            }
        }

        private void OnHideAllUiEvent(bool current, bool previous)
        {
            SetDocumentActive(uiDocument, !current);
        }

        private void OnHideSceneUiEvent(bool enabled, bool previous)
        {
            if (currentScene == null)
                return;

            ECSComponentData<InternalUiContainer>? currentSceneContainer =
                internalUiContainerComponent.GetFor(currentScene, SpecialEntityId.SCENE_ROOT_ENTITY);

            if (currentSceneContainer == null)
                return;

            InternalUiContainer model = currentSceneContainer.Value.model;
            model.rootElement.style.display = new StyleEnum<DisplayStyle>(enabled ? DisplayStyle.Flex : DisplayStyle.None);
        }

        internal static void ApplyParenting(ref HashSet<IParcelScene> scenesToSort, UIDocument uiDocument,
            IInternalECSComponent<InternalUiContainer> internalUiContainerComponent, int currentSceneNumber)
        {
            // Clear previous call so we do not accumulate
            scenesToSort.Clear();

            // check for orphan ui containers
            var allContainers = internalUiContainerComponent.GetForAll();

            for (int i = 0; i < allContainers.Count; i++)
            {
                var uiContainerData = allContainers[i].value;

                // check if ui should be sort (only current and global scenes)
                if (uiContainerData.model.shouldSort
                    && (uiContainerData.scene.isPersistent || uiContainerData.scene.sceneData.sceneNumber == currentSceneNumber))
                {
                    scenesToSort.Add(uiContainerData.scene);
                }

                // add global scenes ui but
                // skip non-global scenes ui containers on root entity since no parenting is needed
                if (uiContainerData.entity.entityId == SpecialEntityId.SCENE_ROOT_ENTITY)
                {
                    var model = uiContainerData.model;

                    if (uiContainerData.scene.isPersistent && model.parentElement == null)
                    {
                        uiDocument.rootVisualElement.Add(model.rootElement);
                        model.parentElement = uiDocument.rootVisualElement;
                        internalUiContainerComponent.PutFor(uiContainerData.scene, uiContainerData.entity, model);
                    }

                    continue;
                }

                // skip containers with parent already set
                if (uiContainerData.model.parentElement != null)
                    continue;

                var parentDataModel = GetParentContainerModel(
                    internalUiContainerComponent,
                    uiContainerData.scene,
                    uiContainerData.model.parentId);

                // apply parenting
                if (parentDataModel != null)
                {
                    var currentContainerModel = uiContainerData.model;
                    parentDataModel.Value.rootElement.Add(uiContainerData.model.rootElement);
                    currentContainerModel.parentElement = parentDataModel.Value.rootElement;

                    internalUiContainerComponent.PutFor(uiContainerData.scene, uiContainerData.model.parentId, parentDataModel.Value);
                    internalUiContainerComponent.PutFor(uiContainerData.scene, uiContainerData.entity, currentContainerModel);
                }
            }
        }

        internal static void ClearCurrentSceneUI(UIDocument uiDocument, IParcelScene currentScene,
            IInternalECSComponent<InternalUiContainer> internalUiContainerComponent)
        {
            ECSComponentData<InternalUiContainer>? currentSceneContainer =
                internalUiContainerComponent.GetFor(currentScene, SpecialEntityId.SCENE_ROOT_ENTITY);

            if (currentSceneContainer == null)
                return;

            InternalUiContainer model = currentSceneContainer.Value.model;
            model.parentElement = null;

            if (uiDocument.rootVisualElement.Contains(model.rootElement))
            {
                uiDocument.rootVisualElement.Remove(model.rootElement);
            }

            internalUiContainerComponent.PutFor(currentScene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
        }

        internal static IParcelScene GetCurrentScene(int sceneNumber, IList<IParcelScene> loadedScenes)
        {
            if (sceneNumber <= 0)
                return null;

            IParcelScene currentScene = null;

            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if (loadedScenes[i].sceneData.sceneNumber == sceneNumber)
                {
                    currentScene = loadedScenes[i];
                    break;
                }
            }

            return currentScene;
        }

        internal static bool ApplySceneUI(IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            UIDocument uiDocument, IParcelScene currentScene, BaseVariable<bool> isSceneUIEnabled)
        {
            ECSComponentData<InternalUiContainer>? sceneRootUiContainer =
                internalUiContainerComponent.GetFor(currentScene, SpecialEntityId.SCENE_ROOT_ENTITY);

            if (sceneRootUiContainer != null)
            {
                var model = sceneRootUiContainer.Value.model;
                uiDocument.rootVisualElement.Insert(0, model.rootElement);
                model.parentElement = uiDocument.rootVisualElement;
                model.rootElement.style.display = new StyleEnum<DisplayStyle>(isSceneUIEnabled.Get() ? DisplayStyle.Flex : DisplayStyle.None);
                internalUiContainerComponent.PutFor(currentScene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
                return true;
            }

            return false;
        }

        private static InternalUiContainer? GetParentContainerModel(IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            IParcelScene scene, long parentId)
        {
            InternalUiContainer? parentDataModel =
                internalUiContainerComponent.GetFor(scene, parentId)?.model;

            // create root entity ui container if needed
            if (parentDataModel == null && parentId == SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                parentDataModel = new InternalUiContainer(parentId);
                var style = parentDataModel.Value.rootElement.style;

                // Initialize with default values
                parentDataModel.Value.rootElement.pickingMode = PickingMode.Ignore; // to avoid blocking pointer
                style.width = new Length(100f, LengthUnit.Percent);
                style.height = new Length(100f, LengthUnit.Percent);
                style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
                style.flexBasis = new StyleLength(StyleKeyword.Auto);
                style.flexGrow = 0;
                style.flexShrink = 1;
                style.flexWrap = new StyleEnum<Wrap>(Wrap.NoWrap);
                style.justifyContent = new StyleEnum<Justify>(Justify.FlexStart);
                style.alignItems = new StyleEnum<Align>(Align.Stretch);
                style.alignSelf = new StyleEnum<Align>(Align.Auto);
                style.alignContent = new StyleEnum<Align>(Align.Stretch);
                style.position = new StyleEnum<Position>(Position.Absolute);
            }

            return parentDataModel;
        }

        internal static void SortSceneUiTree(IInternalECSComponent<InternalUiContainer> internalUiContainerComponent,
            ICollection<IParcelScene> scenesToSort)
        {
            Dictionary<VisualElement, Dictionary<long, RightOfData>> sortSceneTree = new Dictionary<VisualElement, Dictionary<long, RightOfData>>();

            // Setup UI sorting
            var allContainers = internalUiContainerComponent.GetForAll();

            for (int i = 0; i < allContainers.Count; i++)
            {
                var uiContainerData = allContainers[i].value;

                if (!scenesToSort.Contains(uiContainerData.scene))
                    continue;

                InternalUiContainer model = uiContainerData.model;

                // If not parented yet, we skip it
                if (model.parentElement == null)
                    continue;

                // Ignore root scene UI container
                if (uiContainerData.entity.entityId == SpecialEntityId.SCENE_ROOT_ENTITY)
                    continue;

                if (!sortSceneTree.TryGetValue(model.parentElement, out var sceneTreeDictionary))
                {
                    sceneTreeDictionary = new Dictionary<long, RightOfData>();
                    sortSceneTree[model.parentElement] = sceneTreeDictionary;
                }

                sceneTreeDictionary[model.rightOf] = new RightOfData(model.rootElement,
                    uiContainerData.entity.entityId);

                model.shouldSort = false;
                internalUiContainerComponent.PutFor(uiContainerData.scene, uiContainerData.entity.entityId, model);
            }

            // Apply UI sorting
            foreach (var pairs in sortSceneTree)
            {
                VisualElement parentElement = pairs.Key;
                Dictionary<long, RightOfData> sceneSort = pairs.Value;

                int index = 0;
                long nextElementId = 0;

                while (sceneSort.TryGetValue(nextElementId, out RightOfData rightOfData))
                {
                    sceneSort.Remove(nextElementId);
                    rightOfData.element.PlaceInFront(parentElement[index]);
                    nextElementId = rightOfData.id;
                    index++;
                }
            }
        }

        private static void SetDocumentActive(UIDocument uiDocument, bool active)
        {
            uiDocument.rootVisualElement.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private readonly struct RightOfData
        {
            public readonly long id;
            public readonly VisualElement element;

            public RightOfData(VisualElement element, long id)
            {
                this.id = id;
                this.element = element;
            }
        }
    }
}
