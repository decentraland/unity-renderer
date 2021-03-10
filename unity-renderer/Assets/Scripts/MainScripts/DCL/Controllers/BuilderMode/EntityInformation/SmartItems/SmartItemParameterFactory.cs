using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Variables/SmartItemParameterFactory", fileName = "SmartItemParameterFactory", order = 0)]
public class SmartItemParameterFactory : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public SmartItemParameter.ParameterType type;
        public SmartItemUIParameterAdapter prefab;
    }

    [SerializeField] private Entry[] entries;

    public SmartItemUIParameterAdapter GetPrefab(SmartItemParameter.ParameterType type)
    {
        SmartItemUIParameterAdapter adapter = entries.FirstOrDefault(x => x.type == type)?.prefab;
        return adapter;
    }
}
