using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSRuntime
{
    internal static class Utils
    {
        public static bool TryGetComponentData<T>(
            IParcelScene scene,
            long entityId,
            IECSComponent groupComponent,
            IECSComponent updatedComponent,
            out ECSComponentData<T> data)
        {
            if (groupComponent != updatedComponent)
            {
                data = default(ECSComponentData<T>);
                return false;
            }

            try
            {
                if (((ECSComponent<T>)updatedComponent).TryGet(scene, entityId, out data))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogException(e);
#endif
            }

            data = default(ECSComponentData<T>);
            return false;
        }

        public static bool TryGetDataIndex<T1>(List<ECSComponentsGroupData<T1>> list, IDCLEntity entity, out int index)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].entity == entity)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public static bool TryGetDataIndex<T1, T2>(List<ECSComponentsGroupData<T1, T2>> list, IDCLEntity entity, out int index)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].entity == entity)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public static bool TryGetDataIndex<T1, T2, T3>(List<ECSComponentsGroupData<T1, T2, T3>> list, IDCLEntity entity, out int index)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].entity == entity)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }
}
