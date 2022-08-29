using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

public class UIComponentsPlugin : IPlugin
{
    public UIComponentsPlugin()
    {
        IRuntimeComponentFactory factory = Environment.i.world.componentFactory;
        
        // UI
        factory.RegisterBuilder((int) CLASS_ID.UI_INPUT_TEXT_SHAPE, BuildComponent<UIInputText>);
        factory.RegisterBuilder((int) CLASS_ID.UI_FULLSCREEN_SHAPE, BuildComponent<UIScreenSpace>);
        factory.RegisterBuilder((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE, BuildComponent<UIScreenSpace>);
        factory.RegisterBuilder((int) CLASS_ID.UI_CONTAINER_RECT, BuildComponent<UIContainerRect>);
        factory.RegisterBuilder((int) CLASS_ID.UI_SLIDER_SHAPE, BuildComponent<UIScrollRect>);
        factory.RegisterBuilder((int) CLASS_ID.UI_CONTAINER_STACK, BuildComponent<UIContainerStack>);
        factory.RegisterBuilder((int) CLASS_ID.UI_IMAGE_SHAPE, BuildComponent<UIImage>);
        factory.RegisterBuilder((int) CLASS_ID.UI_TEXT_SHAPE, BuildComponent<UIText>);

        factory.createConditions.Add((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE, CanCreateScreenShape);
        factory.createConditions.Add((int) CLASS_ID.UI_FULLSCREEN_SHAPE, CanCreateScreenShape);
    }

    bool CanCreateScreenShape(string sceneId, int classId)
    {
        IParcelScene scene = Environment.i.world.state.GetScene(sceneId);

        if (scene.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>() != null)
            return false;

        return true;
    }


    protected T BuildComponent<T>()
        where T : IComponent, new()
    {
        return new T();
    }


    public void Dispose()
    {
        IRuntimeComponentFactory factory = Environment.i.world.componentFactory;

        factory.UnregisterBuilder((int) CLASS_ID.UI_INPUT_TEXT_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_FULLSCREEN_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_CONTAINER_RECT);
        factory.UnregisterBuilder((int) CLASS_ID.UI_SLIDER_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_CONTAINER_STACK);
        factory.UnregisterBuilder((int) CLASS_ID.UI_IMAGE_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_TEXT_SHAPE);

        factory.createConditions.Remove((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE);
        factory.createConditions.Remove((int) CLASS_ID.UI_FULLSCREEN_SHAPE);
    }
}