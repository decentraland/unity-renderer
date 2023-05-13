using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

public class UUIDEventsPlugin : IPlugin
{
    public PointerEventsController pointerEventsController;
    public InputController_Legacy inputControllerLegacy;
    public InteractionHoverCanvasController hoverCanvas;

    public UUIDEventsPlugin()
    {
        inputControllerLegacy = new InputController_Legacy();
        hoverCanvas = LoadAndInstantiate<InteractionHoverCanvasController>("InteractionHoverCanvas");

        pointerEventsController = new PointerEventsController(inputControllerLegacy, hoverCanvas, SceneReferences.i?.mouseCatcher, DataStore.i.Get<DataStore_World>().currentRaycaster);

        IRuntimeComponentFactory factory = Environment.i.world.componentFactory;

        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_DOWN,
            BuildUUIDEventComponent<OnPointerDown>);
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_UP,
            BuildUUIDEventComponent<OnPointerUp>);
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_CLICK,
            BuildUUIDEventComponent<OnClick>);
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT,
            BuildUUIDEventComponent<OnPointerHoverExit>);
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER,
            BuildUUIDEventComponent<OnPointerHoverEnter>);

        factory.createOverrides.Add((int) CLASS_ID_COMPONENT.UUID_CALLBACK, OnUUIDCallbackIsAdded);
    }

    private void OnUUIDCallbackIsAdded(int sceneNumber, long entityid, ref int classId, object data)
    {
        OnPointerEvent.Model model = new OnPointerEvent.Model();
        if (data is string json)
            model = (OnPointerEvent.Model)model.GetDataFromJSON(json);
        else if (data is Decentraland.Sdk.Ecs6.ComponentBodyPayload payload)
            model = (OnPointerEvent.Model)model.GetDataFromPb(payload);
            
        classId = (int) model.GetClassIdFromType();
    }

    public void Dispose()
    {
        pointerEventsController.Dispose();
        inputControllerLegacy.Dispose();

        IRuntimeComponentFactory factory = Environment.i.world.componentFactory;

        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_DOWN);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_UP);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_CLICK);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER);

        factory.createOverrides.Remove((int) CLASS_ID_COMPONENT.UUID_CALLBACK);
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
