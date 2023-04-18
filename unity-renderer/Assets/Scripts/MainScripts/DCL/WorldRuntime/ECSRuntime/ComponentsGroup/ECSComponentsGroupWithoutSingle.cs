using DCL.Controllers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace DCL.ECSRuntime
{
    public class ECSComponentsGroupWithout<T1> : IECSComponentsGroup, IECSReadOnlyComponentsGroup<T1>
    {
        private readonly IECSComponent component;
        private readonly IECSComponent excludedComponent;
        private readonly List<ECSComponentsGroupData<T1>> list = new List<ECSComponentsGroupData<T1>>();

        IReadOnlyList<ECSComponentsGroupData<T1>> IECSReadOnlyComponentsGroup<T1>.group => list;

        public ECSComponentsGroupWithout(IECSComponent component, IECSComponent excludedComponent)
        {
            Assert.IsNotNull(component, $"component must not be null");
            Assert.IsNotNull(excludedComponent, $"excludedComponent must not be null");
            this.component = component;
            this.excludedComponent = excludedComponent;
        }

        bool IECSComponentsGroup.MatchEntity(IParcelScene scene, IDCLEntity entity)
        {
            return component.HasComponent(scene, entity) && !excludedComponent.HasComponent(scene, entity);
        }

        bool IECSComponentsGroup.ShouldAddOnComponentAdd(IECSComponent component)
        {
            return component == this.component;
        }

        bool IECSComponentsGroup.ShouldRemoveOnComponentRemove(IECSComponent component)
        {
            return component == this.component;
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
            ECSComponentsGroupData<T1> data = new ECSComponentsGroupData<T1>
            (
                scene: scene,
                entity: entity,
                componentData: ((ECSComponent<T1>)component).Get(scene, entity)
            );

            list.Add(data);
        }

        bool IECSComponentsGroup.Remove(IDCLEntity entity)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ECSComponentsGroupData<T1> data = list[i];

                if (data.entity != entity)
                    continue;

                list.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}
