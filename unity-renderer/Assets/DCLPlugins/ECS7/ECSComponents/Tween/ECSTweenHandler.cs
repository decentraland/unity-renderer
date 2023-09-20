using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;

public class ECSTweenHandler : IECSComponentHandler<PBTween>
{
    private IInternalECSComponent<InternalTween> internalTweenComponent;
    private PBTween lastModel;

    public ECSTweenHandler(IInternalECSComponent<InternalTween> internalTweenComponent)
    {
        this.internalTweenComponent = internalTweenComponent;
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        internalTweenComponent.RemoveFor(scene, entity);

        // TODO: Should the state component be removed here?
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBTween model)
    {
        if (lastModel != null && NewModelOnlyChangesPlayingState(lastModel, model))
        {
            // handle 'Playing' state change
            return;
        }

        internalTweenComponent.PutFor(scene, entity, new InternalTween(
            entity.gameObject.transform,
            ProtoConvertUtils.PBVectorToUnityVector(model.Move.Start),
            ProtoConvertUtils.PBVectorToUnityVector(model.Move.End),
            model.Duration,
            !model.HasPlaying || model.Playing));

        lastModel = model;
    }

    private bool NewModelOnlyChangesPlayingState(PBTween oldModel, PBTween newModel)
    {
        return newModel.Playing != oldModel.Playing
               && newModel.Duration.Equals(oldModel.Duration)
               && oldModel.Move.Start.Equals(newModel.Move.Start)
               && oldModel.Move.End.Equals(newModel.Move.End);
    }
}
