using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using UnityEngine;

public class InternalComponentImplementation<ModelType> : IECSComponentImplementation<ModelType>
{
    private readonly ECSComponent<ModelType> component;

    public InternalComponentImplementation(ECSComponent<ModelType> component)
    {
        this.component = component;
    }

    public void SetModel(IParcelScene scene, long entityId, ModelType model)
    {
        if(model is ModelType typedModel)
        {
            component.SetModel(scene, entityId, typedModel);
        }
        else
        {
            // Handle error
            Debug.LogError($"Invalid model type for non-pooled component: {model?.GetType().Name ?? "null"}");
        }
    }

    public void Deserialize(IParcelScene scene, IDCLEntity entity, object message)
    {
        if (component.deserializer != null)
        {
            var model = component.deserializer(message);
            component.SetModel(scene, entity, model);
        }
        else
        {
            // Handle error
            Debug.LogError("Deserializer is not set for non-pooled component");
        }
    }

}
