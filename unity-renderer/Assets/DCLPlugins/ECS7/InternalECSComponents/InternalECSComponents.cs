using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

public class InternalECSComponents : IDisposable, IInternalECSComponents
{
    public IInternalECSComponent<InternalTexturizable> texturizableComponent { private set; get; }
    public IInternalECSComponent<InternalMaterial> materialComponent { private set; get; }

    public InternalECSComponents(ECSComponentsManager componentsManager, ECSComponentsFactory componentsFactory)
    {
        texturizableComponent = new InternalECSComponent<InternalTexturizable>(
            InternalECSComponentsId.TEXTURIZABLE,
            componentsManager,
            componentsFactory,
            () => new InternalTexturizableHandler(() => texturizableComponent));

        materialComponent = new InternalECSComponent<InternalMaterial>(
            InternalECSComponentsId.MATERIAL,
            componentsManager,
            componentsFactory,
            () => new InternalMaterialHandler());
    }

    public void Dispose()
    {
        texturizableComponent.Dispose();
        materialComponent.Dispose();
    }
}