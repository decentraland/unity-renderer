using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class ECSComponentsGroupOfTwo<T1, T2> : IECSComponentsGroup, IECSReadOnlyComponentsGroupOfTwo<T1, T2>
    {
        private readonly IECSComponent component1;
        private readonly IECSComponent component2;
        private readonly List<ECSComponentsGroupOfTwoData<T1, T2>> list = new List<ECSComponentsGroupOfTwoData<T1, T2>>();

        IReadOnlyList<ECSComponentsGroupOfTwoData<T1, T2>> IECSReadOnlyComponentsGroupOfTwo<T1, T2>.group => list;

        public ECSComponentsGroupOfTwo(IECSComponent component1, IECSComponent component2)
        {
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
            ECSComponentsGroupOfTwoData<T1, T2> data = new ECSComponentsGroupOfTwoData<T1, T2>
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
                ECSComponentsGroupOfTwoData<T1, T2> data = list[i];
                if (data.entity != entity)
                    continue;

                list.RemoveAt(i);
                return true;
            }
            return false;
        }
    }
}