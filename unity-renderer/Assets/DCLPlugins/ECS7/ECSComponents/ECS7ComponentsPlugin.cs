using System;
using DCL.ECSComponents;
using DCL.ECSRuntime;

public class ECS7ComponentsPlugin : IDisposable
{

    private readonly ECSTransformComponent transformComponent;

    public ECS7ComponentsPlugin(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter)
    {
        transformComponent = new ECSTransformComponent(1, componentsFactory, componentsWriter);
    }

    public void Dispose()
    {
        transformComponent.Dispose();
    }
}