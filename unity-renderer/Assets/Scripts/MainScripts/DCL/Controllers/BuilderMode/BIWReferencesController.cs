using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWReferencesController
{
    private const string PROJECT_REFERENCES_PATH = "ScriptableObjects/ProjectReferences";
    private const string DYNAMIC_VARIABLE_PATH = "ScriptableObjects/DynamicVariables";

    public BIWProjectReferences projectReferences => projectReferencesValue;
    public BIWDynamicVariables dynamicVariables => dinamycVariablesValue;

    private BIWProjectReferences projectReferencesValue;
    private BIWDynamicVariables dinamycVariablesValue;

    public void Init()
    {
        projectReferencesValue = Resources.Load<BIWProjectReferences>(PROJECT_REFERENCES_PATH);
        dinamycVariablesValue = Resources.Load<BIWDynamicVariables>(DYNAMIC_VARIABLE_PATH);
    }

    public void Dispose()
    {
        projectReferencesValue = null;
        dinamycVariablesValue = null;
    }

}