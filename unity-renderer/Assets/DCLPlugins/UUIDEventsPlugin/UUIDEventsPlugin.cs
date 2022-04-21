using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

public class UUIDEventsPlugin : IPlugin
{
    public PointerEventsController pointerEventsController;
    public InputController_Legacy inputControllerLegacy;
    public InteractionHoverCanvasController hoverCanvas;

    public UUIDEventsPlugin()
    {
        inputControllerLegacy = new InputController_Legacy();
        hoverCanvas = LoadAndInstantiate<InteractionHoverCanvasController>("InteractionHoverCanvas");

        pointerEventsController = new PointerEventsController(inputControllerLegacy, hoverCanvas);

        Environment.i.world.componentFactory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_DOWN,
            BuildUUIDEventComponent<OnPointerDown>);
        Environment.i.world.componentFactory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_UP,
            BuildUUIDEventComponent<OnPointerUp>);
        Environment.i.world.componentFactory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_CLICK,
            BuildUUIDEventComponent<OnClick>);
        Environment.i.world.componentFactory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT,
            BuildUUIDEventComponent<OnPointerHoverExit>);
        Environment.i.world.componentFactory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER,
            BuildUUIDEventComponent<OnPointerHoverEnter>);
    }
    public void Dispose()
    {
        pointerEventsController.Dispose();
        inputControllerLegacy.Dispose();

        Environment.i.world.componentFactory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_DOWN);
        Environment.i.world.componentFactory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_UP);
        Environment.i.world.componentFactory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_CLICK);
        Environment.i.world.componentFactory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT);
        Environment.i.world.componentFactory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER);
    }

    private static T LoadAndInstantiate<T>(string name)
    {
        GameObject instance = Object.Instantiate(Resources.Load(name)) as GameObject;
        instance.name = name;
        return instance.GetComponent<T>();
    }

    private T BuildUUIDEventComponent<T>()
        where T : Component
    {
        var go = new GameObject("UUID Component");
        T newComponent = go.GetOrCreateComponent<T>();
        return newComponent;
    }
}