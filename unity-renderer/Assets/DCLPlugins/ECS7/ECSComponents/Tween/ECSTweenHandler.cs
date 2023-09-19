using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;

public class ECSTweenHandler : IECSComponentHandler<PBTween>
{
    private IInternalECSComponent<InternalTween> internalTweenComponent;

    public ECSTweenHandler(IInternalECSComponent<InternalTween> internalTweenComponent)
    {
        this.internalTweenComponent = internalTweenComponent;
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
    {
    }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {

    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBTween model)
    {

    }
}
