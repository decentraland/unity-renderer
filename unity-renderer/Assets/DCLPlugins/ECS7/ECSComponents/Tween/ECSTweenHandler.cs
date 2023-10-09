using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ECSTweenHandler : IECSComponentHandler<PBTween>
{
    private readonly IInternalECSComponent<InternalTween> internalTweenComponent;
    private readonly IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent;
    private readonly Dictionary<EasingFunction, Ease> easingFunctionsMap = new Dictionary<EasingFunction,Ease>();

    public ECSTweenHandler(IInternalECSComponent<InternalTween> internalTweenComponent, IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent)
    {
        this.internalTweenComponent = internalTweenComponent;
        this.sbcInternalComponent = sbcInternalComponent;
        InitializeEasingFunctionsMap();
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
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
            Tweener tweener = internalComponentModel.tweener;

            if (tweener == null)
            {
                // There may be a tween running for the entity transform, even though internalComponentModel.tweener
                // is null, e.g: during preview mode hot-reload.
                var transformTweens = DOTween.TweensByTarget(entityTransform, true);
                if (transformTweens != null)
                    transformTweens[0].Rewind();
            }
            else
            {
                tweener.Rewind();
            }

            internalComponentModel.transform = entityTransform;
            internalComponentModel.currentTime = model.CurrentTime;

            var ease = Ease.Linear;
            easingFunctionsMap.TryGetValue(model.TweenFunction, out ease);

            switch (model.ModeCase)
            {
                case PBTween.ModeOneofCase.Rotate:
                    entityTransform.localRotation = ProtoConvertUtils.PBQuaternionToUnityQuaternion(model.Rotate.Start);
                    tweener = entityTransform.DOLocalRotateQuaternion(
                        ProtoConvertUtils.PBQuaternionToUnityQuaternion(model.Rotate.End),
                        durationInSeconds).SetEase(ease).SetAutoKill(false);

                    sbcInternalComponent.OnTransformScaleRotationChanged(scene, entity);
                    break;
                case PBTween.ModeOneofCase.Scale:
                    entityTransform.localScale = ProtoConvertUtils.PBVectorToUnityVector(model.Scale.Start);
                    tweener = entityTransform.DOScale(
                        ProtoConvertUtils.PBVectorToUnityVector(model.Scale.End),
                        durationInSeconds).SetEase(ease).SetAutoKill(false);

                    sbcInternalComponent.OnTransformScaleRotationChanged(scene, entity);
                    break;
                case PBTween.ModeOneofCase.Move:
                default:
                    Vector3 startPos = ProtoConvertUtils.PBVectorToUnityVector(model.Move.Start);
                    Vector3 endPos = ProtoConvertUtils.PBVectorToUnityVector(model.Move.End);

                    if (model.Move.HasFaceDirection && model.Move.FaceDirection)
                        entityTransform.forward = (endPos - startPos).normalized;

                    entityTransform.localPosition = startPos;
                    tweener = entityTransform.DOLocalMove(endPos, durationInSeconds)
                                             .SetEase(ease)
                                             .SetAutoKill(false);

                    sbcInternalComponent.SetPosition(scene, entity, entityTransform.position);
                    break;
            }

            tweener.Goto(model.CurrentTime * durationInSeconds, isPlaying);
            internalComponentModel.tweener = tweener;
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

    private bool AreSameModels(PBTween modelA, PBTween modelB)
    {
        if (modelB == null || modelA == null)
            return false;

        if (modelB.ModeCase != modelA.ModeCase
            || modelB.TweenFunction != modelA.TweenFunction
            || !modelB.CurrentTime.Equals(modelA.CurrentTime)
            || !modelB.Duration.Equals(modelA.Duration))
            return false;

        switch (modelA.ModeCase)
        {
            case PBTween.ModeOneofCase.Scale:
                return modelB.Scale.Start.Equals(modelA.Scale.Start)
                       && modelB.Scale.End.Equals(modelA.Scale.End);
            case PBTween.ModeOneofCase.Rotate:
                return modelB.Rotate.Start.Equals(modelA.Rotate.Start)
                       && modelB.Rotate.End.Equals(modelA.Rotate.End);
            case PBTween.ModeOneofCase.Move:
            default:
                return modelB.Move.Start.Equals(modelA.Move.Start)
                       && modelB.Move.End.Equals(modelA.Move.End);
        }
    }

    private void InitializeEasingFunctionsMap()
    {
        easingFunctionsMap[EasingFunction.TfEaseinsine] = Ease.InSine;
        easingFunctionsMap[EasingFunction.TfEaseoutsine] = Ease.OutSine;
        easingFunctionsMap[EasingFunction.TfEasesine] = Ease.InOutSine;
        easingFunctionsMap[EasingFunction.TfEaseinquad] = Ease.InQuad;
        easingFunctionsMap[EasingFunction.TfEaseoutquad] = Ease.OutQuad;
        easingFunctionsMap[EasingFunction.TfEasequad] = Ease.InOutQuad;
        easingFunctionsMap[EasingFunction.TfEaseinexpo] = Ease.InExpo;
        easingFunctionsMap[EasingFunction.TfEaseoutexpo] = Ease.OutExpo;
        easingFunctionsMap[EasingFunction.TfEaseexpo] = Ease.InOutExpo;
        easingFunctionsMap[EasingFunction.TfEaseinelastic] = Ease.InElastic;
        easingFunctionsMap[EasingFunction.TfEaseoutelastic] = Ease.OutElastic;
        easingFunctionsMap[EasingFunction.TfEaseelastic] = Ease.InOutElastic;
        easingFunctionsMap[EasingFunction.TfEaseinbounce] = Ease.InBounce;
        easingFunctionsMap[EasingFunction.TfEaseoutbounce] = Ease.OutBounce;
        easingFunctionsMap[EasingFunction.TfEasebounce] = Ease.InOutBounce;
        easingFunctionsMap[EasingFunction.TfEaseincubic] = Ease.InCubic;
        easingFunctionsMap[EasingFunction.TfEaseoutcubic] = Ease.OutCubic;
        easingFunctionsMap[EasingFunction.TfEasecubic] = Ease.InOutCubic;
        easingFunctionsMap[EasingFunction.TfEaseinquart] = Ease.InQuart;
        easingFunctionsMap[EasingFunction.TfEaseoutquart] = Ease.OutQuart;
        easingFunctionsMap[EasingFunction.TfEasequart] = Ease.InOutQuart;
        easingFunctionsMap[EasingFunction.TfEaseinquint] = Ease.InQuint;
        easingFunctionsMap[EasingFunction.TfEaseoutquint] = Ease.OutQuint;
        easingFunctionsMap[EasingFunction.TfEasequint] = Ease.InOutQuint;
        easingFunctionsMap[EasingFunction.TfEaseincirc] = Ease.InCirc;
        easingFunctionsMap[EasingFunction.TfEaseoutcirc] = Ease.OutCirc;
        easingFunctionsMap[EasingFunction.TfEasecirc] = Ease.InOutCirc;
        easingFunctionsMap[EasingFunction.TfEaseinback] = Ease.InBack;
        easingFunctionsMap[EasingFunction.TfEaseoutback] = Ease.OutBack;
        easingFunctionsMap[EasingFunction.TfEaseback] = Ease.InOutBack;
    }
}
