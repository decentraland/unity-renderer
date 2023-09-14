using DCL.Controllers;
using DCL.ECS7.ComponentWrapper;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using UnityEngine;

public class PooledComponentImplementation<ModelType> : IECSComponentImplementation<ModelType>
{
    private readonly ECSComponent<ModelType> component;

    public PooledComponentImplementation(ECSComponent<ModelType> component)
    {
        this.component = component;
    }

    public void SetModel(IParcelScene scene, long entityId, ModelType model)
    {
        if(model != null)
        {
            component.SetModel(scene, entityId, model);
        }
        else
        {
            // Handle error
            Debug.LogError($"Invalid model type for pooled component: {model?.GetType().Name ?? "null"}");
        }
    }

    // public void Deserialize(IParcelScene scene, IDCLEntity entity, object message)
    // {
    //     // Since we are using pooling, we need to get an instance from the pool
    //     ModelType modelInstance = component.iEcsComponentPool.Get();
    //     Debug.Log($"FD:: Model instance type: {modelInstance?.GetType().Name ?? "null"}");
    //
    //     // Wrap our modelInstance in a ProtobufWrappedComponent for deserialization
    //     var wrappedComponent = new ProtobufWrappedComponent<ModelType>(modelInstance);
    //
    //     // Check if the message is a byte array and then deserialize
    //     if (message is byte[] byteArray)
    //     {
    //         wrappedComponent.DeserializeFrom(byteArray);
    //     }
    //     else
    //     {
    //         Debug.LogError($"Unexpected message type: {message?.GetType().Name ?? "null"}. Expected byte array.");
    //         return;
    //     }
    //
    //     component.SetModel(scene, entity, modelInstance);
    // }

    public void Deserialize(IParcelScene scene, IDCLEntity entity, object message)
    {
        // Since we are using pooling, we need to get an instance from the pool
        ModelType modelInstance = component.iEcsComponentPool.Get();

        if (message is byte[] byteArray)
        {
            // Use reflection to create a ProtobufWrappedComponent for the given ModelType
            Type wrappedComponentType = typeof(ProtobufWrappedComponent<>).MakeGenericType(modelInstance.GetType());
            object wrappedComponentInstance = Activator.CreateInstance(wrappedComponentType, modelInstance);

            // Use reflection to call the DeserializeFrom method on the created instance
            var method = wrappedComponentType.GetMethod("DeserializeFrom");

            method.Invoke(wrappedComponentInstance, new object[] { byteArray });
        }

        component.SetModel(scene, entity, modelInstance);
    }

}
