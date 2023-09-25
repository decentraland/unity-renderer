using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Interface;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.TweenSystem
{
    public class ECSTweenSystem
    {
        private readonly IInternalECSComponent<InternalTween> tweenInternalComponent;
        private readonly ECSComponent<PBTweenState> tweenStateComponent;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool;
        private readonly WrappedComponentPool<IWrappedComponent<ECSTransform>> transformComponentPool;
        private readonly Vector3Variable worldOffset;

        // TODO: Remove when polishing PR
        private const string QUERY_PARAM_SYNC_TWEENSTATE = "SYNC_TWEENSTATE";

        public ECSTweenSystem(IInternalECSComponent<InternalTween> tweenInternalComponent,
            ECSComponent<PBTweenState> tweenStateComponent,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool,
            WrappedComponentPool<IWrappedComponent<ECSTransform>> transformComponentPool,
            Vector3Variable worldOffset)
        {
            this.tweenInternalComponent = tweenInternalComponent;
            this.tweenStateComponent = tweenStateComponent;
            this.componentsWriter = componentsWriter;
            this.tweenStateComponentPool = tweenStateComponentPool;
            this.transformComponentPool = transformComponentPool;
            this.worldOffset = worldOffset;
        }

        public void Update()
        {
            var tweenComponentGroup = tweenInternalComponent.GetForAll();
            int entitiesCount = tweenComponentGroup.Count;

            for (int i = 0; i < entitiesCount; i++)
            {
                var tweenStatePooledComponent = tweenStateComponentPool.Get();
                var tweenStateComponentModel = tweenStatePooledComponent.WrappedComponent.Model;
                IParcelScene scene = tweenComponentGroup[i].key1;
                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                long entity = tweenComponentGroup[i].key2;
                InternalTween model = tweenComponentGroup[i].value.model;

                if (model.removed)
                {
                    model.tweener.Kill();
                    writer.Remove(entity, ComponentID.TWEEN_STATE);
                    continue;
                }

                float currentTime = model.tweener.ElapsedPercentage();
                if (currentTime.Equals(1f) && model.currentTime.Equals(1f))
                    continue;

                bool playing = model.playing;

                // TODO: Remove if we don't need it...
                if (WebInterface.CheckURLParam(QUERY_PARAM_SYNC_TWEENSTATE) &&
                    model.currentTime == 0 &&
                    tweenStateComponent.TryGet(scene, entity, out var existentTweenState))
                {
                    playing = model.playing = existentTweenState.model.State != TweenStateStatus.TsPaused;
                    currentTime = model.currentTime = existentTweenState.model.CurrentTime;

                    Debug.Log($"ENGINE TWEEN SYSTEM - Previous tween state applied to new Tween - current-time:{currentTime}; state-status:{existentTweenState.model.State}");
                    model.tweener.Goto(currentTime * model.tweener.Duration(), playing);
                }

                if (playing)
                {
                    tweenStateComponentModel.CurrentTime = currentTime;
                    tweenStateComponentModel.State = currentTime.Equals(1f) ? TweenStateStatus.TsCompleted : TweenStateStatus.TsActive;
                }
                else
                {
                    tweenStateComponentModel.State = TweenStateStatus.TsPaused;
                }

                //TODO: If we decide to make TweenState a GOVS component instead of LWW, we have to use Append() here
                writer.Put(entity, ComponentID.TWEEN_STATE, tweenStatePooledComponent);

                UpdateTransformComponent(scene, entity, writer, model);

                model.currentTime = currentTime;
                tweenInternalComponent.PutFor(scene, entity, model);
            }
        }

        private void UpdateTransformComponent(IParcelScene scene, long entity, ComponentWriter writer, InternalTween internalTweenModel)
        {
            // TODO: Update rotation and scale when we add support for that.
            var transformComponent = transformComponentPool.Get();
            var transformComponentModel = transformComponent.WrappedComponent.Model;
            Vector3 currentWorldOffset = worldOffset.Get();
            var newPosition = internalTweenModel.transform.position;
            transformComponentModel.position = UtilsScene.GlobalToScenePosition(ref scene.sceneData.basePosition, ref newPosition, ref currentWorldOffset);
            writer.Put(entity, ComponentID.TRANSFORM, transformComponent);
        }
    }
}
