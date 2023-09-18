

using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.TweenSystem
{
    public class ECSTweenSystem
    {
        private readonly IInternalECSComponent<InternalTween> tweenComponent;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;

        public ECSTweenSystem(IInternalECSComponent<InternalTween> tweenComponent, IReadOnlyDictionary<int, ComponentWriter> componentsWriter)
        {
            this.tweenComponent = tweenComponent;
            this.componentsWriter = componentsWriter;
        }

        public void Update()
        {
            var tweenComponentGroup = tweenComponent.GetForAll();
            int entitiesCount = tweenComponentGroup.Count;

            for (int i = 0; i < entitiesCount; i++)
            {
                InternalTween model = tweenComponentGroup[i].value.model;

                model.currentInterpolationTime += model.calculatedSpeed * Time.deltaTime;
                if (model.currentInterpolationTime > 1)
                    model.currentInterpolationTime = 1;

                // TODO: Support a collection and not only 2 points
                Vector3.Lerp(model.tweenPoints[0], model.tweenPoints[1], model.currentInterpolationTime);

                // ...
            }
        }
    }
}
