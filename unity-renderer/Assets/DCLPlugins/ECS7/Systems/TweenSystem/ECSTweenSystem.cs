
using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Interface;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.TweenSystem
{
    public class ECSTweenSystem
    {
        private readonly IInternalECSComponent<InternalTween> tweenComponent;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool;
        private readonly WrappedComponentPool<IWrappedComponent<ECSTransform>> transformComponentPool;
        private readonly Vector3Variable worldOffset;

        public ECSTweenSystem(IInternalECSComponent<InternalTween> tweenComponent,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool,
            WrappedComponentPool<IWrappedComponent<ECSTransform>> transformComponentPool,
            Vector3Variable worldOffset)
        {
            this.tweenComponent = tweenComponent;
            this.componentsWriter = componentsWriter;
            this.tweenStateComponentPool = tweenStateComponentPool;
            this.transformComponentPool = transformComponentPool;
            this.worldOffset = worldOffset;
        }

        public void Update()
        {
            var tweenComponentGroup = tweenComponent.GetForAll();
            int entitiesCount = tweenComponentGroup.Count;

            for (int i = 0; i < entitiesCount; i++)
            {
                IParcelScene scene = tweenComponentGroup[i].key1;
                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                long entity = tweenComponentGroup[i].key2;
                InternalTween model = tweenComponentGroup[i].value.model;

                if (model.removed)
                {
                    writer.Remove(entity, ComponentID.TWEEN_STATE);
                    continue;
                }

                if (model.currentTime.Equals(1f))
                    continue;

                var tweenStateComponent = tweenStateComponentPool.Get();
                var tweenStateComponentModel = tweenStateComponent.WrappedComponent.Model;

                if (model.playing)
                {
                    model.currentTime += model.calculatedSpeed * Time.deltaTime;
                    if (model.currentTime > 1f)
                        model.currentTime = 1f;
                    tweenStateComponentModel.CurrentTime = model.currentTime;

                    Vector3 startPos = WorldStateUtils.ConvertPointInSceneToUnityPosition(model.startPosition, scene);
                    Vector3 endPos = WorldStateUtils.ConvertPointInSceneToUnityPosition(model.endPosition, scene);

                    model.transform.position = Vector3.Lerp(startPos, endPos, model.currentTime);

                    tweenComponent.PutFor(scene, entity, model);

                    tweenStateComponentModel.State = model.currentTime.Equals(1f) ? TweenState.TsCompleted : TweenState.TsActive;
                }
                else
                {
                    tweenStateComponentModel.State = TweenState.TsPaused;
                }

                //TODO: If we decide to make TweenState a GOVS component instead of LWW, we have to use Append() here
                writer.Put(entity, ComponentID.TWEEN_STATE, tweenStateComponent);

                // Update Transform component
                // TODO: Update rotation and scale when we add support for that.
                var transformComponent = transformComponentPool.Get();
                var transformComponentModel = transformComponent.WrappedComponent.Model;
                Vector3 currentWorldOffset = worldOffset.Get();
                var newPosition = model.transform.position;
                transformComponentModel.position = UtilsScene.GlobalToScenePosition(ref scene.sceneData.basePosition, ref newPosition, ref currentWorldOffset);
                writer.Put(entity, ComponentID.TRANSFORM, transformComponent);
            }
        }
    }
}
