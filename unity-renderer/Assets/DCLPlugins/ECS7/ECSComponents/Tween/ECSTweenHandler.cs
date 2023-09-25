using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DG.Tweening;
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
            Transform entityTransform = entity.gameObject.transform;
            float durationInSeconds = model.Duration / 1000;

            internalComponentModel.transform = entityTransform;
            internalComponentModel.currentTime = model.CurrentTime;

            // TODO: Evaluate if we need local values instead of global...
            // Move to start position
            entityTransform.position = ProtoConvertUtils.PBVectorToUnityVector(model.Move.Start);
            var tweener = entityTransform.DOMove(
                ProtoConvertUtils.PBVectorToUnityVector(model.Move.End),
                durationInSeconds).SetEase(SDKEasingFunctinToDOTweenEaseType(model.TweenFunction)).SetAutoKill(false);

            tweener.Goto(model.CurrentTime * durationInSeconds, isPlaying);

            internalComponentModel.tweener = tweener;
        }

        internalComponentModel.playing = isPlaying;
        internalTweenComponent.PutFor(scene, entity, internalComponentModel);

        lastModel = model;
    }

    private Ease SDKEasingFunctinToDOTweenEaseType(EasingFunction easingFunction)
    {
        switch (easingFunction)
        {
            case EasingFunction.TfLinear:
            default:
                return Ease.Linear;
                break;
        }
    }

    private bool IsSameAsLastModel(PBTween targetModel)
    {
        return (lastModel != null
                && lastModel.CurrentTime.Equals(targetModel.CurrentTime)
                && lastModel.Duration.Equals(targetModel.Duration)
                && lastModel.Move.Start.Equals(targetModel.Move.Start)
                && lastModel.Move.End.Equals(targetModel.Move.End)
                && lastModel.TweenFunction.Equals(targetModel.TweenFunction));
    }
}
