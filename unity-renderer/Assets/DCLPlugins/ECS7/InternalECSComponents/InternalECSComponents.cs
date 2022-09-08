using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

public class InternalECSComponents : IDisposable, IInternalECSComponents
{
    public IInternalECSComponent<InternalTexturizable> texturizableComponent { get; }
    public IInternalECSComponent<InternalMaterial> materialComponent { get; }

    public IInternalECSComponent<InternalColliders> onPointerColliderComponent { get; }
    public IInternalECSComponent<InternalColliders> physicColliderComponent { get; }

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

        onPointerColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_POINTER,
            componentsManager,
            componentsFactory,
            null);

        physicColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_PHYSICAL,
            componentsManager,
            componentsFactory,
            null);
    }

    public void Dispose()
    {
        texturizableComponent.Dispose();
        materialComponent.Dispose();
    }
}