using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartItemListView : MonoBehaviour
{
    [SerializeField] private SmartItemParameterFactory factory;

    List<DCLBuilderInWorldEntity> entitiesList = new List<DCLBuilderInWorldEntity>();

    List<GameObject> childrenList = new List<GameObject>();

    public void SetSmartItemParameters(SmartItemParameter[] parameters, Dictionary<object,object> smartItemValues)
    {
        for(int i = 0; i <childrenList.Count;i++)
        {
            Destroy(childrenList[i]);
        }

        gameObject.SetActive(true);

        foreach (SmartItemParameter parameter in parameters)
        {
            SmartItemUIParameterAdapter prefabToInstantiate = factory.GetPrefab(parameter.GetParameterType());
            InstantiateParameter(parameter, smartItemValues, prefabToInstantiate);
        }
    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> entitiesList)
    {
        this.entitiesList = BuilderInWorldUtils.RemoveGroundEntities(entitiesList);
    }

    void InstantiateParameter(SmartItemParameter parameter, Dictionary<object, object> smartItemValues, SmartItemUIParameterAdapter parameterAdapterPrefab)
    {
        SmartItemUIParameterAdapter parameterAdapter = Instantiate(parameterAdapterPrefab.gameObject, transform).GetComponent<SmartItemUIParameterAdapter>();

        IEntityListHandler entityListHanlder = parameterAdapter.GetComponent<IEntityListHandler>();
        if(entityListHanlder != null)
            entityListHanlder.SetEntityList(entitiesList);

        parameterAdapter.SetParameter(parameter,smartItemValues);
        childrenList.Add(parameterAdapter.gameObject);
    }
}