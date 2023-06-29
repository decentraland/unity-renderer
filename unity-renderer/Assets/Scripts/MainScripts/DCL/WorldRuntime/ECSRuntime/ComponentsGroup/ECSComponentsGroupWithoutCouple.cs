using DCL.Controllers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace DCL.ECSRuntime
{
    public class ECSComponentsGroupWithout<T1, T2> : IECSComponentsGroup, IECSReadOnlyComponentsGroup<T1, T2>
    {
        private readonly IECSComponent component1;
        private readonly IECSComponent component2;
        private readonly IECSComponent excludedComponent;
        private readonly List<ECSComponentsGroupData<T1, T2>> list = new List<ECSComponentsGroupData<T1, T2>>();

        IReadOnlyList<ECSComponentsGroupData<T1, T2>> IECSReadOnlyComponentsGroup<T1, T2>.group => list;

        public ECSComponentsGroupWithout(IECSComponent component1, IECSComponent component2, IECSComponent excludedComponent)
        {
            Assert.IsNotNull(component1, $"component1 must not be null");
            Assert.IsNotNull(component2, $"component2 must not be null");
            Assert.IsNotNull(excludedComponent, $"excludedComponent must not be null");
            this.component1 = component1;
            this.component2 = component2;
            this.excludedComponent = excludedComponent;
        }

        bool IECSComponentsGroup.MatchEntity(IParcelScene scene, IDCLEntity entity)
        {
            return component1.HasComponent(scene, entity)
                   && component2.HasComponent(scene, entity)
                   && !excludedComponent.HasComponent(scene, entity);
        }

        bool IECSComponentsGroup.ShouldAddOnComponentAdd(IECSComponent component)
        {
            return component == component1 || component == component2;
        }

        bool IECSComponentsGroup.ShouldRemoveOnComponentRemove(IECSComponent component)
        {
            return component == component1 || component == component2;
        }

        bool IECSComponentsGroup.ShouldAddOnComponentRemove(IECSComponent component)
        {
            return component == excludedComponent;
        }

        bool IECSComponentsGroup.ShouldRemoveOnComponentAdd(IECSComponent component)
        {
            return component == excludedComponent;
        }

        void IECSComponentsGroup.Add(IParcelScene scene, IDCLEntity entity)
        {
            ((ECSComponent<T1>)component1).TryGet(scene, entity.entityId, out var componentData1);
            ((ECSComponent<T2>)component2).TryGet(scene, entity.entityId, out var componentData2);

            ECSComponentsGroupData<T1, T2> data = new ECSComponentsGroupData<T1, T2>
            (
                scene: scene,
                entity: entity,
                componentData1: componentData1,
                componentData2: componentData2
            );

            list.Add(data);
        }

        bool IECSComponentsGroup.Remove(IDCLEntity entity)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ECSComponentsGroupData<T1, T2> data = list[i];

                if (data.entity != entity)
                    continue;

                list.RemoveAt(i);
                return true;
            }

            return false;
        }

        void IECSComponentsGroup.Update(IParcelScene scene, IDCLEntity entity, IECSComponent component)
        {
            long entityId = entity.entityId;
            if (!Utils.TryGetDataIndex(list, entity, out int index))
                return;

            if (Utils.TryGetComponentData<T1>(scene, entityId, component1, component, out var d1))
                list[index] = list[index].With(d1);
            if (Utils.TryGetComponentData<T2>(scene, entityId, component2, component, out var d2))
                list[index] = list[index].With(d2);
        }
    }
}
