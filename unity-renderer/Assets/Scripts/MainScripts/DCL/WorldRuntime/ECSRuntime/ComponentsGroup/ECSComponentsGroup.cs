using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine.Assertions;

namespace DCL.ECSRuntime
{
    public class ECSComponentsGroup<T1, T2> : IECSComponentsGroup, IECSReadOnlyComponentsGroup<T1, T2>
    {
        private readonly IECSComponent component1;
        private readonly IECSComponent component2;
        private readonly List<ECSComponentsGroupData<T1, T2>> list = new List<ECSComponentsGroupData<T1, T2>>();

        IReadOnlyList<ECSComponentsGroupData<T1, T2>> IECSReadOnlyComponentsGroup<T1, T2>.group => list;

        public ECSComponentsGroup(IECSComponent component1, IECSComponent component2)
        {
            Assert.IsNotNull(component1, $"component1 must not be null");
            Assert.IsNotNull(component2, $"component2 must not be null");
            this.component1 = component1;
            this.component2 = component2;
        }

        bool IECSComponentsGroup.Match(IParcelScene scene, IDCLEntity entity)
        {
            return component1.HasComponent(scene, entity) && component2.HasComponent(scene, entity);
        }

        bool IECSComponentsGroup.Match(IECSComponent component)
        {
            return component == component1 || component == component2;
        }

        void IECSComponentsGroup.Add(IParcelScene scene, IDCLEntity entity)
        {
            ECSComponentsGroupData<T1, T2> data = new ECSComponentsGroupData<T1, T2>
            (
                scene: scene,
                entity: entity,
                componentData1: ((ECSComponent<T1>)component1).Get(scene, entity),
                componentData2: ((ECSComponent<T2>)component2).Get(scene, entity)
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
    }
}