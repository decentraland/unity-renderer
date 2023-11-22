using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static DCL.ECSComponents.EasingFunction;
using static DG.Tweening.Ease;

public class ECSTweenHandler : IECSComponentHandler<PBTween>
{
    private static readonly Dictionary<EasingFunction, Ease> easingFunctionsMap = new Dictionary<EasingFunction,Ease>()
    {
        [EfLinear] = Linear,
        [EfEaseinsine] = InSine,
        [EfEaseoutsine] = OutSine,
        [EfEasesine] = InOutSine,
        [EfEaseinquad] = InQuad,
        [EfEaseoutquad] = OutQuad,
        [EfEasequad] = InOutQuad,
        [EfEaseinexpo] = InExpo,
        [EfEaseoutexpo] = OutExpo,
        [EfEaseexpo] = InOutExpo,
        [EfEaseinelastic] = InElastic,
        [EfEaseoutelastic] = OutElastic,
        [EfEaseelastic] = InOutElastic,
        [EfEaseinbounce] = InBounce,
        [EfEaseoutbounce] = OutBounce,
        [EfEasebounce] = InOutBounce,
        [EfEaseincubic] = InCubic,
        [EfEaseoutcubic] = OutCubic,
        [EfEasecubic] = InOutCubic,
        [EfEaseinquart] = InQuart,
        [EfEaseoutquart] = OutQuart,
        [EfEasequart] = InOutQuart,
        [EfEaseinquint] = InQuint,
        [EfEaseoutquint] = OutQuint,
        [EfEasequint] = InOutQuint,
        [EfEaseincirc] = InCirc,
        [EfEaseoutcirc] = OutCirc,
        [EfEasecirc] = InOutCirc,
        [EfEaseinback] = InBack,
        [EfEaseoutback] = OutBack,
        [EfEaseback] = InOutBack
    };

    private readonly IInternalECSComponent<InternalTween> internalTweenComponent;
    private readonly IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent;
    private Tweener currentTweener;

    public ECSTweenHandler(IInternalECSComponent<InternalTween> internalTweenComponent, IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent)
    {
        this.internalTweenComponent = internalTweenComponent;
        this.sbcInternalComponent = sbcInternalComponent;
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        currentTweener.Kill(false);
        internalTweenComponent.RemoveFor(scene, entity, new InternalTween()
        {
            removed = true
        });

        // SBC Internal Component is reset when the Transform component is removed, not here.
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBTween model)
    {
        if (model.ModeCase == PBTween.ModeOneofCase.None)
            return;

        // by default it's playing
        bool isPlaying = !model.HasPlaying || model.Playing;

        var internalComponentModel = internalTweenComponent.GetFor(scene, entity)?.model ?? new InternalTween();
        if (!AreSameModels(model, internalComponentModel.lastModel))
        {
            Transform entityTransform = entity.gameObject.transform;
            float durationInSeconds = model.Duration / 1000;
            currentTweener = internalComponentModel.tweener;

            if (currentTweener == null)
            {
                // There may be a tween running for the entity transform, even though internalComponentModel.tweener
                // is null, e.g: during preview mode hot-reload.
                var transformTweens = DOTween.TweensByTarget(entityTransform, true);
                transformTweens?[0].Rewind(false);
            }
            else
            {
                currentTweener.Rewind(false);
            }

            internalComponentModel.transform = entityTransform;
            internalComponentModel.currentTime = model.CurrentTime;

            if (!easingFunctionsMap.TryGetValue(model.EasingFunction, out Ease ease))
                ease = Ease.Linear;

            switch (model.ModeCase)
            {
                case PBTween.ModeOneofCase.Rotate:
                    currentTweener = SetupRotationTween(scene, entity,
                        ProtoConvertUtils.PBQuaternionToUnityQuaternion(model.Rotate.Start),
                        ProtoConvertUtils.PBQuaternionToUnityQuaternion(model.Rotate.End),
                        durationInSeconds, ease);
                    break;
                case PBTween.ModeOneofCase.Scale:
                    currentTweener = SetupScaleTween(scene, entity,
                        ProtoConvertUtils.PBVectorToUnityVector(model.Scale.Start),
                        ProtoConvertUtils.PBVectorToUnityVector(model.Scale.End),
                        durationInSeconds, ease);
                    break;
                case PBTween.ModeOneofCase.Move:
                default:
                    currentTweener = SetupPositionTween(scene, entity,
                        ProtoConvertUtils.PBVectorToUnityVector(model.Move.Start),
                        ProtoConvertUtils.PBVectorToUnityVector(model.Move.End),
                        durationInSeconds, ease, model.Move.HasFaceDirection && model.Move.FaceDirection);
                    break;
            }

            currentTweener.Goto(model.CurrentTime * durationInSeconds, isPlaying);
            internalComponentModel.tweener = currentTweener;
            internalComponentModel.tweenMode = model.ModeCase;
        }
        else if (internalComponentModel.playing == isPlaying)
        {
            return;
        }

        internalComponentModel.playing = isPlaying;

        if (isPlaying)
            internalComponentModel.tweener.Play();
        else
            internalComponentModel.tweener.Pause();

        internalComponentModel.lastModel = model;
        internalTweenComponent.PutFor(scene, entity, internalComponentModel);
    }

    private static bool AreSameModels(PBTween modelA, PBTween modelB)
    {
        if (modelB == null || modelA == null)
            return false;

        if (modelB.ModeCase != modelA.ModeCase
            || modelB.EasingFunction != modelA.EasingFunction
            || !modelB.CurrentTime.Equals(modelA.CurrentTime)
            || !modelB.Duration.Equals(modelA.Duration))
            return false;

        return modelA.ModeCase switch
               {
                   PBTween.ModeOneofCase.Scale => modelB.Scale.Start.Equals(modelA.Scale.Start) && modelB.Scale.End.Equals(modelA.Scale.End),
                   PBTween.ModeOneofCase.Rotate => modelB.Rotate.Start.Equals(modelA.Rotate.Start) && modelB.Rotate.End.Equals(modelA.Rotate.End),
                   PBTween.ModeOneofCase.Move => modelB.Move.Start.Equals(modelA.Move.Start) && modelB.Move.End.Equals(modelA.Move.End),
                   PBTween.ModeOneofCase.None => modelB.Move.Start.Equals(modelA.Move.Start) && modelB.Move.End.Equals(modelA.Move.End),
                   _ => modelB.Move.Start.Equals(modelA.Move.Start) && modelB.Move.End.Equals(modelA.Move.End)
               };
    }

    private Tweener SetupRotationTween(IParcelScene scene, IDCLEntity entity, Quaternion startRotation,
        Quaternion endRotation, float durationInSeconds, Ease ease)
    {
        var entityTransform = entity.gameObject.transform;
        entityTransform.localRotation = startRotation;
        var tweener = entityTransform.DOLocalRotateQuaternion(endRotation, durationInSeconds).SetEase(ease).SetAutoKill(false);

        sbcInternalComponent.OnTransformScaleRotationChanged(scene, entity);

        return tweener;
    }

    private Tweener SetupScaleTween(IParcelScene scene, IDCLEntity entity, Vector3 startScale,
        Vector3 endScale, float durationInSeconds, Ease ease)
    {
        var entityTransform = entity.gameObject.transform;
        entityTransform.localScale = startScale;
        var tweener = entityTransform.DOScale(endScale, durationInSeconds).SetEase(ease).SetAutoKill(false);

        sbcInternalComponent.OnTransformScaleRotationChanged(scene, entity);

        return tweener;
    }

    private Tweener SetupPositionTween(IParcelScene scene, IDCLEntity entity, Vector3 startPosition,
        Vector3 endPosition, float durationInSeconds, Ease ease, bool faceDirection)
    {
        var entityTransform = entity.gameObject.transform;

        if (faceDirection)
            entityTransform.forward = (endPosition - startPosition).normalized;

        entityTransform.localPosition = startPosition;
        var tweener = entityTransform.DOLocalMove(endPosition, durationInSeconds).SetEase(ease).SetAutoKill(false);

        sbcInternalComponent.SetPosition(scene, entity, entityTransform.position);

        return tweener;
    }
}
