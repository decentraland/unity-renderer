﻿using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.UI;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace ECSSystems.CameraSystem
{
    public struct VisualElementRepresentation
    {
        public long entityId;
        public long parentId;
        public VisualElement visualElement;
        public VisualElement parentVisualElement;
    }
    
    public class CanvasPainter
    {
        private const string UI_DOCUMENT_PREFAB_PATH = "RootNode";
        private const int FRAMES_PER_PAINT = 10;
        
        private readonly UIDocument rootNode;
        private readonly UIDataContainer dataContainer;
        private readonly RendererState rendererState;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly IUpdateEventHandler updateEventHandler;
        private readonly ECSComponentsManager componentsManager;
        
        private UISceneDataContainer sceneDataContainerToUse;
        private IParcelScene scene;

        private Dictionary<long, VisualElementRepresentation> visualElements = new Dictionary<long, VisualElementRepresentation>();
        private List<VisualElementRepresentation> orphanVisualElements = new List<VisualElementRepresentation>();
        
        private IECSReadOnlyComponentsGroup<PBUiTransform, PBUiTextShape> textComponentGroup;

        private int framesCounter = 0;
        
        public CanvasPainter(DataStore_ECS7 dataStoreEcs7, RendererState rendererState, IUpdateEventHandler updateEventHandler, ECSComponentsManager componentsManager)
        {
            this.updateEventHandler = updateEventHandler;
            this.loadedScenes = dataStoreEcs7.scenes;
            this.rendererState = rendererState;
            this.dataContainer = dataStoreEcs7.uiDataContainer;
            var prefab = Resources.Load<UIDocument>(UI_DOCUMENT_PREFAB_PATH);
            rootNode = GameObject.Instantiate(prefab);
            
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);

            // Group Components 
            textComponentGroup = componentsManager.CreateComponentGroup<PBUiTransform, PBUiTextShape>(ComponentID.UI_TRANSFORM, ComponentID.UI_TEXT);
            
#if UNITY_EDITOR
            rootNode.name = "Scene Canvas";
#endif
        }
        
        public void Dispose()
        {
            GameObject.Destroy(rootNode.gameObject);
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            sceneDataContainerToUse.OnUITransformRemoved -= RemoveVisualElement;
        }

        private void SetScene(IParcelScene scene)
        {
            this.scene = scene;
            
            sceneDataContainerToUse.OnUITransformRemoved -= RemoveVisualElement;
            
            // We get the UI information of the scene to paint their canvas 
            sceneDataContainerToUse = dataContainer.GetDataContainer(scene);
            sceneDataContainerToUse.OnUITransformRemoved += RemoveVisualElement;
        }

        private void RemoveVisualElement(IDCLEntity entity)
        {
            // We try to get the parent
            if (visualElements.TryGetValue(entity.entityId, out VisualElementRepresentation visualElementRepresentantion))
            {
                visualElementRepresentantion.parentVisualElement.Remove(visualElementRepresentantion.visualElement);
                visualElements.Remove(entity.entityId);
            }
        }

        private void Update()
        {
            framesCounter++;
            if (!rendererState.Get() || framesCounter < FRAMES_PER_PAINT)
                return;

            framesCounter = 0;
            
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if(loadedScenes[i].sceneData.id != Environment.i.world.state.currentSceneId)
                    continue;
                
                DrawUI(loadedScenes[i]);
                break;
            }
        }

        private void DrawUI(IParcelScene scene)
        {
            if(this.scene?.sceneData.id != scene.sceneData.id)
                SetScene(scene);

            DrawUI();
        }
        
        public void DrawUI()
        {
            if (!sceneDataContainerToUse.IsDirty())
                return;
            
            VisualElement root = rootNode.rootVisualElement;
            
            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            // VisualElement label = new Label("Hello World! From C#");
            // label.style.width = 200;
            // label.style.height = 200;
            // label.style.alignSelf = Align.Center;
            // label.style.backgroundColor = Color.green;
            // root.Add(label);

            //Register callback
            //visualElement.RegisterCallback<ClickEvent>(ev => Debug.Log("Clicked"));

            List<long> entitiesWithRendeerComponent = new List<long>();
            IECSReadOnlyComponentData<PBUiTransform> componentData;
            
            foreach (VisualElementRepresentation visualElementRepresentation in  orphanVisualElements)
            {
                SetParent(root, visualElementRepresentation, false);
                
                // If we found the parent, we remove it and set it visible
                if (visualElementRepresentation.parentVisualElement != null)
                {
                    visualElementRepresentation.visualElement.visible = true;
                    orphanVisualElements.Remove(visualElementRepresentation);
                }
            }
            
            // We create all the text elements
            foreach (var textComponent in textComponentGroup.group)
            {
                componentData = textComponent.componentData1;
                
                // We create the text element
                var visualElement = TransformToVisualElement(componentData.model, new TextElement());
                visualElements[componentData.entity.entityId] = new VisualElementRepresentation()
                {
                    entityId = componentData.entity.entityId,
                    parentId = componentData.entity.parentId,
                    visualElement = visualElement
                };
                
                // We add the entity to a list so it doesn't create a visual element for the PBUiTransform since the transform is inside the text element
                entitiesWithRendeerComponent.Add(componentData.entity.entityId);
            }
            
            // We create all the elements that doesn't have a rendering ( Image, Text...etc) since we need them to position them correctly
            foreach (var kvp in sceneDataContainerToUse.sceneCanvasTransform)
            {
                var entity = scene.entities[kvp.Key];

                // If we already have a rendering element, we skip since don't need to create it again
                if (entitiesWithRendeerComponent.Contains(entity.entityId))
                    continue;
                
                // We create the element to position the rendering element correctly
                var visualElement = TransformToVisualElement(kvp.Value, new Image());
                visualElements[entity.entityId] = new VisualElementRepresentation()
                {
                    entityId = entity.entityId,
                    parentId = entity.parentId,
                    visualElement = visualElement
                };
            }
            
            // We set the parenting
            foreach (KeyValuePair<long,VisualElementRepresentation> kvp in visualElements)
            {
                SetParent(root, kvp.Value);
            }
        }

        private void SetParent(VisualElement root, VisualElementRepresentation visualElementRepresentation, bool addToOrphanList = true) 
        {
            // if the parent is the root canvas, we assign to the root
            if (visualElementRepresentation.parentId == SpecialEntityId.SCENE_CANVAS_ROOT)
            {
                root.Add(visualElementRepresentation.visualElement);
                visualElementRepresentation.parentVisualElement = root;
            }
            else
            {
                // We try to get the parent
                if (visualElements.TryGetValue(visualElementRepresentation.parentId, out VisualElementRepresentation parentVisualElementRepresentantion))
                {
                    parentVisualElementRepresentantion.visualElement.Add(visualElementRepresentation.visualElement);
                    visualElementRepresentation.parentVisualElement = parentVisualElementRepresentantion.visualElement;
                }
                else if(addToOrphanList)
                { 
                    // There is no father so it is orphan right now. We can try to find the father the next time, we hide it until we find it
                    visualElementRepresentation.visualElement.visible = false;
                    orphanVisualElements.Add(visualElementRepresentation);
                }
            }
        }
        
        private VisualElement TransformToVisualElement(PBUiTransform model, VisualElement element)
        {
            // Flex
            element.style.flexDirection = GetFlexDirection(model.FlexDirection);
            element.style.flexBasis = model.FlexBasis;
            element.style.flexGrow = model.FlexGrow;
            element.style.flexShrink = model.FlexShrink;
            element.style.flexWrap = GetWrap(model.FlexWrap);

            // Align 
            element.style.alignContent = GetAlign(model.AlignContent);
            element.style.alignItems = GetAlign(model.AlignItems);
            element.style.alignSelf = GetAlign(model.AlignSelf);
            element.style.justifyContent = GetJustify(model.JustifyContent);
            
            // Layout
            element.style.height = model.Height;
            element.style.width = model.Width;

            element.style.maxWidth = model.MaxWidth;
            element.style.maxHeight = model.MaxHeight;
            
            element.style.minHeight = model.MinHeight;
            element.style.minWidth = model.MinWidth;

            element.style.paddingBottom = model.PaddingBottom;
            element.style.paddingLeft = model.PaddingLeft;
            element.style.paddingRight = model.PaddingRight;
            element.style.paddingTop = model.PaddingTop;

            element.style.borderBottomWidth = model.BorderBottom;
            element.style.borderLeftWidth = model.BorderLeft;
            element.style.borderRightWidth = model.BorderRight;
            element.style.borderTopWidth = model.BorderTop;

            element.style.marginLeft = model.MarginLeft;
            element.style.marginRight = model.MarginRight;
            element.style.marginBottom = model.MarginBottom;
            element.style.marginTop = model.MarginTop;
            
                // Position
            element.style.position = GetPosition(model.PositionType);

            element.style.backgroundColor = Random.ColorHSV();

            return element;
        }

        private static StyleEnum<Justify> GetJustify (YGJustify justify)
        {
            switch (justify)
            {
                case YGJustify.FlexStart:
                    return Justify.FlexStart;
                case YGJustify.Center:
                    return Justify.Center;
                case YGJustify.FlexEnd:
                    return Justify.FlexEnd;
                case YGJustify.SpaceBetween:
                    return Justify.SpaceBetween;
                case YGJustify.SpaceAround:
                    return Justify.SpaceAround;
                default:
                    return Justify.FlexStart;
            }
        }
        
        private static StyleEnum<Wrap> GetWrap (YGWrap wrap)
        {
            switch (wrap)
            {
                case YGWrap.NoWrap:
                    return Wrap.NoWrap;
                case YGWrap.Wrap:
                    return Wrap.Wrap;
                case YGWrap.WrapReverse:
                    return Wrap.WrapReverse;
                default:
                    return Wrap.Wrap;
            }
        }
        
        private static StyleEnum<FlexDirection> GetFlexDirection (YGFlexDirection direction)
        {
            switch (direction)
            {
                case YGFlexDirection.Column:
                    return FlexDirection.Column;
                case YGFlexDirection.ColumnReverse:
                    return FlexDirection.ColumnReverse;
                case YGFlexDirection.Row:
                    return FlexDirection.Row;
                case YGFlexDirection.RowReverse:
                    return FlexDirection.RowReverse;
                default:
                    return FlexDirection.Column;
            }
        }

        private static StyleEnum<Position> GetPosition(YGPositionType  positionType)
        {
            switch (positionType)
            {
                case YGPositionType.Relative:
                    return Position.Relative;
                case YGPositionType.Absolute:
                    return Position.Absolute;
                default:
                    return Position.Relative;
            }
        }
        
        private static StyleEnum<Align> GetAlign(YGAlign align)
        {
            switch (align)
            {
                case YGAlign.Auto:
                    return Align.Auto;
                case YGAlign.FlexStart:
                    return Align.FlexStart;
                case YGAlign.Center:
                    return Align.Center;
                case YGAlign.FlexEnd:
                    return Align.FlexEnd;
                default:
                    return Align.Auto;
            }
        }
    }
}