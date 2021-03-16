using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component describes the visibility of the Entity in the builder in world.
/// Builder in World send a message to kernel to change the value of this component in order to show/hide it
/// </summary>
public class DCLVisibleOnEdit : BaseDisposable
{
    [System.Serializable]
    public class Model : BaseModel
    {
        public bool isVisible;

        public override BaseModel GetDataFromJSON(string json)
        {
            return Utils.SafeFromJson<Model>(json);
        }
    }

    public DCLVisibleOnEdit()
    {
        model = new Model();
    }

    public override int GetClassId()
    {
        return (int) CLASS_ID.VISIBLE_ON_EDIT;
    }

    public override IEnumerator ApplyChanges(BaseModel newModel)
    {
        RaiseOnAppliedChanges();
        return null;
    }
}