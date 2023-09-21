
using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.TweenSystem
{
    public class ECSTweenSystem
    {
        private readonly IInternalECSComponent<InternalTween> tweenComponent;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<PBTweenState>> componentPool;

        public ECSTweenSystem(IInternalECSComponent<InternalTween> tweenComponent,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBTweenState>> componentPool)
        {
            this.tweenComponent = tweenComponent;
            this.componentsWriter = componentsWriter;
            this.componentPool = componentPool;
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

                // Update TweenState component (TODO: Should it be a GOVS or a LWW?);
                var tweenStateComponent = componentPool.Get();
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
                writer.Put(entity, ComponentID.TWEEN_STATE, tweenStateComponent);

                // TODO: Update Transform component

                // TODO: When is the state component removed?

            }
        }
    }
}
