using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

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
        internalTweenComponent.RemoveFor(scene, entity, new InternalTween()
        {
            removed = true
        });
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBTween model)
    {
        // by default it's playing
        bool isPlaying = !model.HasPlaying || model.Playing;

        var internalComponentModel = internalTweenComponent.GetFor(scene, entity)?.model ?? new InternalTween();

        if (!IsSameAsLastModel(model))
        {
            internalComponentModel.currentTime = model.CurrentTime;
            internalComponentModel.transform = entity.gameObject.transform;
            internalComponentModel.startPosition = ProtoConvertUtils.PBVectorToUnityVector(model.Move.Start);
            internalComponentModel.endPosition = ProtoConvertUtils.PBVectorToUnityVector(model.Move.End);
            internalComponentModel.durationInMilliseconds = model.Duration;
        }
        internalComponentModel.playing = isPlaying;
        internalComponentModel.UpdateSpeedCalculation();
        internalTweenComponent.PutFor(scene, entity, internalComponentModel);

        lastModel = model;
    }

    private bool IsSameAsLastModel(PBTween targetModel)
    {
        return (lastModel != null
                && lastModel.CurrentTime.Equals(targetModel.CurrentTime)
                && lastModel.Duration.Equals(targetModel.Duration)
                && lastModel.Move.Start.Equals(targetModel.Move.Start)
                && lastModel.Move.End.Equals(targetModel.Move.End));
    }
}
