using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ECSTweenHandler : IECSComponentHandler<PBTween>
{
    private IInternalECSComponent<InternalTween> internalTweenComponent;
    private readonly WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool;
    private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
    // private readonly IECSComponentWriter componentsWriter;
    private PBTween lastModel;

    public ECSTweenHandler(
        IInternalECSComponent<InternalTween> internalTweenComponent,
        WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool,
        IReadOnlyDictionary<int, ComponentWriter> componentsWriter)
        // IECSComponentWriter componentsWriter)
    {
        this.internalTweenComponent = internalTweenComponent;
        this.tweenStateComponentPool = tweenStateComponentPool;
        this.componentsWriter = componentsWriter;
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
        if (model.ModeCase == PBTween.ModeOneofCase.None)
            return;

        // by default it's playing
        bool isPlaying = !model.HasPlaying || model.Playing;

        var internalComponentModel = internalTweenComponent.GetFor(scene, entity)?.model ?? new InternalTween();

        if (!IsSameAsLastModel(model))
        {
            Transform entityTransform = entity.gameObject.transform;
            float durationInSeconds = model.Duration / 1000;
            Tweener tweener = internalComponentModel.tweener;

            if (tweener == null)
            {
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

            // TODO: Evaluate if we need to use local or global values...
            switch (model.ModeCase)
            {
                case PBTween.ModeOneofCase.Rotate:
                    entityTransform.localRotation = ProtoConvertUtils.PBQuaternionToUnityQuaternion(model.Rotate.Start);
                    tweener = entityTransform.DOLocalRotateQuaternion(
                        ProtoConvertUtils.PBQuaternionToUnityQuaternion(model.Rotate.End),
                        durationInSeconds).SetEase(SDKEasingFunctinToDOTweenEaseType(model.TweenFunction)).SetAutoKill(false);
                    break;
                case PBTween.ModeOneofCase.Scale:
                    entityTransform.localScale = ProtoConvertUtils.PBVectorToUnityVector(model.Scale.Start);
                    tweener = entityTransform.DOScale(
                        ProtoConvertUtils.PBVectorToUnityVector(model.Scale.End),
                        durationInSeconds).SetEase(SDKEasingFunctinToDOTweenEaseType(model.TweenFunction)).SetAutoKill(false);
                    break;
                case PBTween.ModeOneofCase.Move:
                default:
                    entityTransform.localPosition = ProtoConvertUtils.PBVectorToUnityVector(model.Move.Start);
                    tweener = entityTransform.DOLocalMove(
                        ProtoConvertUtils.PBVectorToUnityVector(model.Move.End),
                        durationInSeconds).SetEase(SDKEasingFunctinToDOTweenEaseType(model.TweenFunction)).SetAutoKill(false);
                    break;
            }

            tweener.Goto(model.CurrentTime * durationInSeconds, isPlaying);
            internalComponentModel.tweener = tweener;

            if (componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
            {
                // Explanation on this FRESH Tween State put here:
                // Something that may be happening: TweenSystem runs -> TweenHandler receives an updated component
                // -> Next Frame Tween System Runs (but the scene already read the last TweenState
                var tweenStatePooledComponent = tweenStateComponentPool.Get();
                PBTweenState tweenStateComponentModel = tweenStatePooledComponent.WrappedComponent.Model;
                tweenStateComponentModel.CurrentTime = model.CurrentTime;
                if (isPlaying)
                    tweenStateComponentModel.State = model.CurrentTime.Equals(1f) ? TweenStateStatus.TsCompleted : TweenStateStatus.TsActive;
                else
                    tweenStateComponentModel.State = TweenStateStatus.TsPaused;

                writer.Put(entity.entityId, ComponentID.TWEEN_STATE, tweenStatePooledComponent);

                // 'IECSComponentWriter componentsWriter' doesn't work ¯\_(ツ)_/¯
                // componentsWriter.PutComponent(
                //     scene,
                //     entity,
                //     ComponentID.TWEEN_STATE,
                //     tweenStateComponentModel,
                //     ECSComponentWriteType.SEND_TO_SCENE);
            }
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

        internalTweenComponent.PutFor(scene, entity, internalComponentModel);
        lastModel = model;
    }

    private bool IsSameAsLastModel(PBTween targetModel)
    {
        if (lastModel == null)
            return false;

        if (lastModel.ModeCase != targetModel.ModeCase
            || lastModel.TweenFunction != targetModel.TweenFunction
            || !lastModel.CurrentTime.Equals(targetModel.CurrentTime)
            || !lastModel.Duration.Equals(targetModel.Duration))
            return false;

        switch (targetModel.ModeCase)
        {
            case PBTween.ModeOneofCase.Scale:
                return lastModel.Scale.Start.Equals(targetModel.Scale.Start)
                       && lastModel.Scale.End.Equals(targetModel.Scale.End);
            case PBTween.ModeOneofCase.Rotate:
                return lastModel.Rotate.Start.Equals(targetModel.Rotate.Start)
                       && lastModel.Rotate.End.Equals(targetModel.Rotate.End);
            case PBTween.ModeOneofCase.Move:
            default:
                return lastModel.Move.Start.Equals(targetModel.Move.Start)
                       && lastModel.Move.End.Equals(targetModel.Move.End);
        }
    }

    private Ease SDKEasingFunctinToDOTweenEaseType(EasingFunction easingFunction)
    {
        switch (easingFunction)
        {
            case EasingFunction.TfEaseinsine:
                return Ease.InSine;
            case EasingFunction.TfEaseoutsine:
                return Ease.OutSine;
            case EasingFunction.TfEasesine:
                return Ease.InOutSine;
            case EasingFunction.TfEaseinquad:
                return Ease.InQuad;
            case EasingFunction.TfEaseoutquad:
                return Ease.OutQuad;
            case EasingFunction.TfEasequad:
                return Ease.InOutQuad;
            case EasingFunction.TfEaseinexpo:
                return Ease.InExpo;
            case EasingFunction.TfEaseoutexpo:
                return Ease.OutExpo;
            case EasingFunction.TfEaseexpo:
                return Ease.InOutExpo;
            case EasingFunction.TfEaseinelastic:
                return Ease.InElastic;
            case EasingFunction.TfEaseoutelastic:
                return Ease.OutElastic;
            case EasingFunction.TfEaseelastic:
                return Ease.InOutElastic;
            case EasingFunction.TfEaseinbounce:
                return Ease.InBounce;
            case EasingFunction.TfEaseoutbounce:
                return Ease.OutBounce;
            case EasingFunction.TfEasebounce:
                return Ease.InOutBounce;
            case EasingFunction.TfEaseincubic:
                return Ease.InCubic;
            case EasingFunction.TfEaseoutcubic:
                return Ease.OutCubic;
            case EasingFunction.TfEasecubic:
                return Ease.InOutCubic;
            case EasingFunction.TfEaseinquart:
                return Ease.InQuart;
            case EasingFunction.TfEaseoutquart:
                return Ease.OutQuart;
            case EasingFunction.TfEasequart:
                return Ease.InOutQuart;
            case EasingFunction.TfEaseinquint:
                return Ease.InQuint;
            case EasingFunction.TfEaseoutquint:
                return Ease.OutQuint;
            case EasingFunction.TfEasequint:
                return Ease.InOutQuint;
            case EasingFunction.TfEaseincirc:
                return Ease.InCirc;
            case EasingFunction.TfEaseoutcirc:
                return Ease.OutCirc;
            case EasingFunction.TfEasecirc:
                return Ease.InOutCirc;
            case EasingFunction.TfEaseinback:
                return Ease.InBack;
            case EasingFunction.TfEaseoutback:
                return Ease.OutBack;
            case EasingFunction.TfEaseback:
                return Ease.InOutBack;
            case EasingFunction.TfLinear:
            default:
                return Ease.Linear;
        }
    }
}
