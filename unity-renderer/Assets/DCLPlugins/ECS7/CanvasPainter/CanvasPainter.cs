using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7.UI;
using DCL.ECSComponents;
using DCL.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace ECSSystems.CameraSystem
{
    public class CanvasPainter
    {
        private const string UI_DOCUMENT_PREFAB_PATH = "RootNode";
        
        private readonly UIDocument rootNode;
        private readonly UIDataContainer dataContainer;
        private readonly RendererState rendererState;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly IUpdateEventHandler updateEventHandler;
        
        private UISceneDataContainer sceneDataContainerToUse;
        private IParcelScene scene;

        private Dictionary<long, VisualElement> transforms = new Dictionary<long, VisualElement>();
        private Dictionary<long, VisualElement> transformsParent = new Dictionary<long, VisualElement>();
        
        public CanvasPainter(DataStore_ECS7 dataStoreEcs7, RendererState rendererState, IUpdateEventHandler updateEventHandler )
        {
            this.updateEventHandler = updateEventHandler;
            this.loadedScenes = dataStoreEcs7.scenes;
            this.rendererState = rendererState;
            this.dataContainer = dataStoreEcs7.uiDataContainer;
            var prefab = Resources.Load<UIDocument>(UI_DOCUMENT_PREFAB_PATH);
            rootNode = GameObject.Instantiate(prefab);
            
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            
#if UNITY_EDITOR
            rootNode.name = "Scene Canvas";
#endif
        }
        
        public void Dispose()
        {
            GameObject.Destroy(rootNode.gameObject);
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        }

        private void SetScene(IParcelScene scene)
        {
            this.scene = scene;
            this.sceneDataContainerToUse = dataContainer.GetDataContainer(scene);
        }

        public void Update()
        {
            if (!rendererState.Get())
                return;
            
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if(loadedScenes[i].sceneData.id != Environment.i.world.state.currentSceneId)
                    continue;
                
                DrawUI(loadedScenes[i]);
                break;
            }
        }

        public void DrawUI(IParcelScene scene)
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
            
            Dictionary<IDCLEntity, VisualElement> orphanVisualElements = new Dictionary<IDCLEntity, VisualElement>();
            foreach (var kvp in sceneDataContainerToUse.sceneCanvasTransform)
            {
                var entity = scene.entities[kvp.Key];
                var visualElement = TransformToVisualElement(kvp.Value);
                
                transforms[entity.entityId] = visualElement;
                
                // if it is the root 
                if (entity.parent == null)
                {
                    root.Add(visualElement);
                }
                else
                {
                    if (transforms.TryGetValue(entity.parentId, out VisualElement parentElement))
                    {
                        transformsParent[entity.entityId] = parentElement;
                        parentElement.Add(visualElement);
                    }
                    else
                    {
                        orphanVisualElements[entity] = visualElement;
                    }
                }
            }
            
            foreach (var kvp in  orphanVisualElements)
            {
                if (transforms.TryGetValue(kvp.Key.parentId, out VisualElement parentElement))
                {
                    transformsParent[kvp.Key.entityId] = parentElement;
                    parentElement.Add(kvp.Value);
                }
                else
                {
                    Debug.Log("A UITransform element doesn't have a father");
                }
            }
        }
        
        private static VisualElement TransformToVisualElement(PBUiTransform model)
        {
            VisualElement element = new Image();

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