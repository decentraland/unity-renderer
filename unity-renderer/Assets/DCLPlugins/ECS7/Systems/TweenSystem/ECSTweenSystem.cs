
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

                model.currentTime += model.calculatedSpeed * Time.deltaTime;
                if (model.currentTime > 1)
                    model.currentTime = 1;

                // UtilsScene.GlobalToScenePosition()
                Vector3 startPos = WorldStateUtils.ConvertPointInSceneToUnityPosition(model.startPosition, scene);
                Vector3 endPos = WorldStateUtils.ConvertPointInSceneToUnityPosition(model.endPosition, scene);

                model.transform.position = Vector3.Lerp(startPos, endPos, model.currentTime);

                // Update internal component
                tweenComponent.PutFor(scene, entity, model);

                // Update TweenState component (TODO: Should it be a GOVS or a LWW?);
                var pooledComponent = componentPool.Get();
                var pooledComponentModel = pooledComponent.WrappedComponent.Model;
                pooledComponentModel.State = TweenState.TsActive;

                // TODO: change when the protocol is fixed
                pooledComponentModel.CurrentTime = (int)model.currentTime;

                writer.Put(entity, ComponentID.TWEEN_STATE, pooledComponent);

                // TODO: Update Transform component

                // TODO: When is the state component removed?

            }
        }
    }
}
