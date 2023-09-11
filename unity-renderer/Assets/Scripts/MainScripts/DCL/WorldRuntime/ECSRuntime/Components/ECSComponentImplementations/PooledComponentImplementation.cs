using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using UnityEngine;

public class PooledComponentImplementation<ModelType> : IEcsComponentImplementation<ModelType>
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

    public void Deserialize(IParcelScene scene, IDCLEntity entity, object message)
    {
        // Since we are using pooling, we need to get an instance from the pool
        ModelType modelInstance = component.iEcsComponentPool.Get();

        // FD:: do we need to do further deserialization here due to message?


        component.SetModel(scene, entity, modelInstance);
    }
}
