using System;
using System.Collections.Generic;
using System.Linq;
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
using Environment = DCL.Environment;
using Random = UnityEngine.Random;

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
        
        private IECSReadOnlyComponentsGroup<PBUiTransform, PBUiText> textComponentGroup;

        private int framesCounter = 0;
        private bool canvasCleared = false;
        
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
            textComponentGroup = componentsManager.CreateComponentGroup<PBUiTransform, PBUiText>(ComponentID.UI_TRANSFORM, ComponentID.UI_TEXT);
            
#if UNITY_EDITOR
            rootNode.name = "Scene Canvas";
#endif
        }
        
        public void Dispose()
        {
            GameObject.Destroy(rootNode.gameObject);
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            if(sceneDataContainerToUse != null)
                sceneDataContainerToUse.OnUITransformRemoved -= RemoveVisualElement;
        }

        private void SetScene(IParcelScene scene)
        {
            this.scene = scene;
            
            if(sceneDataContainerToUse != null)
                sceneDataContainerToUse.OnUITransformRemoved -= RemoveVisualElement;
            
            // We get the UI information of the scene to paint their canvas 
            sceneDataContainerToUse = dataContainer.GetDataContainer(scene);
            sceneDataContainerToUse.OnUITransformRemoved += RemoveVisualElement;
            ClearAndHideRootLayout();
        }

        private void ClearAndHideRootLayout()
        {
            rootNode.rootVisualElement.visible = false;
            rootNode.rootVisualElement.Clear();
            canvasCleared = true;
        }

        private void RemoveVisualElement(IDCLEntity entity)
        {
            // We try to get the parent
            if (visualElements.TryGetValue(entity.entityId, out VisualElementRepresentation visualElementRepresentantion))
            {
                if (visualElementRepresentantion.parentVisualElement != null)
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
            bool sceneFound = false;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if(loadedScenes[i].sceneData.id != Environment.i.world.state.currentSceneId)
                    continue;
                sceneFound = true;
                DrawUI(loadedScenes[i]);
                break;
            }
            
            // If we are entering in an empty parcel, or ECS6 scene, we should also clear the UI
            if (!sceneFound && !canvasCleared)
                ClearAndHideRootLayout();
        }

        private void DrawUI(IParcelScene scene)
        {
            if(this.scene != scene || canvasCleared)
                SetScene(scene);

            DrawUI();
        }
        
        public void DrawUI()
        {
            // If the canvas hasn't change or if there is no canvas we skip
            if (!canvasCleared && (!sceneDataContainerToUse.IsDirty() || !sceneDataContainerToUse.sceneCanvasTransform.Any()))
                return;

            canvasCleared = false;
            sceneDataContainerToUse.UIRendered();
            
            VisualElement root = rootNode.rootVisualElement;
            
            Debug.Log("Pintando sobre un canvas de width: " + Screen.width +"   height: "+ Screen.height);
            

            //Register callback
            //visualElement.RegisterCallback<ClickEvent>(ev => Debug.Log("Clicked"));

            List<long> entitiesWithRendeerComponent = new List<long>();
            IECSReadOnlyComponentData<PBUiTransform> componentData;
            
            foreach (VisualElementRepresentation visualElementRepresentation in  orphanVisualElements)
            {
                SetParent(visualElementRepresentation, false);
                
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

                var textElement = new TextElement();
                textElement.text = textComponent.componentData2.model.Text;
                var color = textComponent.componentData2.model.TextColor;
                textElement.style.color =  new UnityEngine.Color(color.R, color.G,color.B,1);
                
                // We create the text element
                var visualElement = TransformToVisualElement(componentData.model, textElement, false);

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
                // If the entity doesn't exist we skip
                if(!scene.entities.TryGetValue(kvp.Key, out IDCLEntity entity))
                    continue;

                // If we already have a rendering element, we skip since don't need to create it again
                if (entitiesWithRendeerComponent.Contains(entity.entityId))
                    continue;
                
                // We ensure that the canvas is visible
                if (entity.parentId == SpecialEntityId.SCENE_ROOT_ENTITY)
                    root.visible = true;
              
                // We create the element to position the rendering element correctly
                var visualElement = TransformToVisualElement(kvp.Value,  new Image());
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
                SetParent(kvp.Value);
            }
        }

        private void SetParent(VisualElementRepresentation visualElementRepresentation, bool addToOrphanList = true)
        {
            // if the parent is the root canvas, it doesn't have parent, we skip
            if (visualElementRepresentation.parentId == SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                rootNode.rootVisualElement.Add(visualElementRepresentation.visualElement);
                visualElementRepresentation.parentVisualElement = rootNode.rootVisualElement;
            }
            else
            {
                // We try to get the parent
                if (visualElements.TryGetValue(visualElementRepresentation.parentId, out VisualElementRepresentation parentVisualElementRepresentantion))
                {
                    parentVisualElementRepresentantion.visualElement.Add(visualElementRepresentation.visualElement);
                    visualElementRepresentation.parentVisualElement = parentVisualElementRepresentantion.visualElement;
                }
                else if (addToOrphanList)
                {
                    // There is no father so it is orphan right now. We can try to find the father the next time, we hide it until we find it
                    visualElementRepresentation.visualElement.visible = false;
                    orphanVisualElements.Add(visualElementRepresentation);
                }
            }
        }

        private VisualElement TransformToVisualElement(PBUiTransform model, VisualElement element, bool randomColor = true)
        {
            element.style.display = GetDisplay(model.Display);
            element.style.overflow = GetOverflow(model.Overflow);

            // Flex
            element.style.flexDirection = GetFlexDirection(model.FlexDirection);
            if (!float.IsNaN(model.FlexBasis))
                element.style.flexBasis = new Length(model.FlexBasis, GetUnit(model.FlexBasisUnit));

            element.style.flexGrow = model.FlexGrow;
            element.style.flexShrink = model.FlexShrink;
            element.style.flexWrap = GetWrap(model.FlexWrap);
            element.style.position = GetPosition(model.PositionType);

            // Align 
            if (model.AlignContent != YGAlign.FlexStart)
                element.style.alignContent = GetAlign(model.AlignContent);
            if (model.AlignItems != YGAlign.Auto)
                element.style.alignItems = GetAlign(model.AlignItems);
            if (model.AlignSelf != YGAlign.Auto)
                element.style.alignSelf = GetAlign(model.AlignSelf);
            element.style.justifyContent = GetJustify(model.JustifyContent);

            // Layout size
            if (!float.IsNaN(model.Height))
                element.style.height = new Length(model.Height, GetUnit(model.HeightUnit));
            if (!float.IsNaN(model.Width))
                element.style.width = new Length(model.Width, GetUnit(model.WidthUnit));

            if (!float.IsNaN(model.MaxWidth))
                element.style.maxWidth = new Length(model.MaxWidth, GetUnit(model.MaxWidthUnit));
            if (!float.IsNaN(model.MaxHeight))
                element.style.maxHeight = new Length(model.MaxHeight, GetUnit(model.MaxHeightUnit));

            if (!float.IsNaN(model.MinHeight))
                element.style.minHeight = new Length(model.MinHeight, GetUnit(model.MinHeightUnit));
            if (!float.IsNaN(model.MinWidth))
                element.style.minWidth = new Length(model.MinWidth, GetUnit(model.MinWidthUnit));

            // Paddings
            if (!Mathf.Approximately(model.PaddingBottom, 0))
                element.style.paddingBottom = new Length(model.PaddingBottom, GetUnit(model.PaddingBottomUnit));
            if (!Mathf.Approximately(model.PaddingLeft, 0))
                element.style.paddingLeft = new Length(model.PaddingLeft, GetUnit(model.PaddingLeftUnit));
            if (!Mathf.Approximately(model.PaddingRight, 0))
                element.style.paddingRight = new Length(model.PaddingRight, GetUnit(model.PaddingRightUnit));
            if (!Mathf.Approximately(model.PaddingTop, 0))
                element.style.paddingTop = new Length(model.PaddingTop, GetUnit(model.PaddingTopUnit));
            
            // Margins
            if (!Mathf.Approximately(model.MarginLeft, 0))
                element.style.marginLeft = new Length(model.MarginLeft, GetUnit(model.MarginLeftUnit));
            if (!Mathf.Approximately(model.MarginRight, 0))
                element.style.marginRight = new Length(model.MarginRight, GetUnit(model.MarginRightUnit));
            if (!Mathf.Approximately(model.MarginBottom, 0))
                element.style.marginBottom = new Length(model.MarginBottom, GetUnit(model.MarginBottomUnit));
            if (!Mathf.Approximately(model.MarginTop, 0))
                element.style.marginTop = new Length(model.MarginTop, GetUnit(model.MarginTopUnit));
            
            // Borders
            element.style.borderBottomWidth = model.BorderBottom;
            element.style.borderLeftWidth = model.BorderLeft;
            element.style.borderRightWidth = model.BorderRight;
            element.style.borderTopWidth = model.BorderTop;

            // Position
            element.style.position = GetPosition(model.PositionType);

            // This is for debugging purposes, we will change this to a proper approach so devs can debug the UI easily.
            if (randomColor)
                element.style.backgroundColor = Random.ColorHSV();

            return element;
        }

        private LengthUnit GetUnit(YGUnit unit)
        {
            switch (unit)
            {
                case YGUnit.Point:
                    return LengthUnit.Pixel;
                case YGUnit.Percent:
                    return LengthUnit.Percent;
                default:
                    return LengthUnit.Pixel;
            }
        }

        private StyleEnum<Overflow> GetOverflow (YGOverflow overflow)
        {
            switch (overflow)
            {
                case YGOverflow.Visible:
                    return Overflow.Visible;
                case YGOverflow.Hidden:
                    return Overflow.Hidden;
                default:
                    return Overflow.Visible;
            }
        }
        
        private StyleEnum<DisplayStyle> GetDisplay (YGDisplay display)
        {
            switch (display)
            {
                case YGDisplay.Flex:
                    return DisplayStyle.Flex;
                    break;
                case YGDisplay.None:
                    return DisplayStyle.None;
                default:
                    return DisplayStyle.Flex;
            }
        }

        private StyleEnum<Justify> GetJustify (YGJustify justify)
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
        
        private StyleEnum<Wrap> GetWrap (YGWrap wrap)
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
        
        private StyleEnum<FlexDirection> GetFlexDirection (YGFlexDirection direction)
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
                    return FlexDirection.Row;
            }
        }

        private StyleEnum<Position> GetPosition(YGPositionType  positionType)
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
        
        private StyleEnum<Align> GetAlign(YGAlign align)
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
                case YGAlign.Stretch:
                    return Align.Stretch;
                default:
                    return Align.Auto;
            }
        }
    }
}