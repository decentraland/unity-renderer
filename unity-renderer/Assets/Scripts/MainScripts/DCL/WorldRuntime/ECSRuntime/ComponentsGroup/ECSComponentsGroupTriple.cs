using DCL.Controllers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace DCL.ECSRuntime
{
    public class ECSComponentsGroup<T1, T2, T3> : IECSComponentsGroup, IECSReadOnlyComponentsGroup<T1, T2, T3>
    {
        private readonly IECSComponent component1;
        private readonly IECSComponent component2;
        private readonly IECSComponent component3;
        private readonly List<ECSComponentsGroupData<T1, T2, T3>> list = new List<ECSComponentsGroupData<T1, T2, T3>>();

        IReadOnlyList<ECSComponentsGroupData<T1, T2, T3>> IECSReadOnlyComponentsGroup<T1, T2, T3>.group => list;

        public ECSComponentsGroup(IECSComponent component1, IECSComponent component2, IECSComponent component3)
        {
            Assert.IsNotNull(component1, $"component1 must not be null");
            Assert.IsNotNull(component2, $"component2 must not be null");
            Assert.IsNotNull(component3, $"component3 must not be null");
            this.component1 = component1;
            this.component2 = component2;
            this.component3 = component3;
        }

        bool IECSComponentsGroup.MatchEntity(IParcelScene scene, IDCLEntity entity)
        {
            return component1.HasComponent(scene, entity)
                   && component2.HasComponent(scene, entity)
                   && component3.HasComponent(scene, entity);
        }

        bool IECSComponentsGroup.ShouldAddOnComponentAdd(IECSComponent component)
        {
            return component == component1 || component == component2 || component == component3;
        }

        bool IECSComponentsGroup.ShouldRemoveOnComponentRemove(IECSComponent component)
        {
            return component == component1 || component == component2 || component == component3;
        }

        bool IECSComponentsGroup.ShouldAddOnComponentRemove(IECSComponent component)
        {
            return false;
        }

        bool IECSComponentsGroup.ShouldRemoveOnComponentAdd(IECSComponent component)
        {
            return false;
        }

        void IECSComponentsGroup.Add(IParcelScene scene, IDCLEntity entity)
        {
            ((ECSComponent<T1>)component1).TryGet(scene, entity.entityId, out var componentData1);
            ((ECSComponent<T2>)component2).TryGet(scene, entity.entityId, out var componentData2);
            ((ECSComponent<T3>)component3).TryGet(scene, entity.entityId, out var componentData3);

            ECSComponentsGroupData<T1, T2, T3> data = new ECSComponentsGroupData<T1, T2, T3>
            (
                scene: scene,
                entity: entity,
                componentData1: componentData1,
                componentData2: componentData2,
                componentData3: componentData3
            );

            list.Add(data);
        }

        bool IECSComponentsGroup.Remove(IDCLEntity entity)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ECSComponentsGroupData<T1, T2, T3> data = list[i];

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
            if (Utils.TryGetComponentData<T3>(scene, entityId, component3, component, out var d3))
                list[index] = list[index].With(d3);

        }
    }
}
