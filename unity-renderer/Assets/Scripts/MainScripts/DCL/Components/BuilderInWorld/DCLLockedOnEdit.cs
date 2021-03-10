using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component describes the lock status of the Entity in the builder in world.
/// Builder in World send a message to kernel to change the value of this component in order to lock/unlock it
/// </summary>
public class DCLLockedOnEdit : BaseDisposable
{
    [System.Serializable]
    public class Model : BaseModel
    {
        public bool isLocked;
        
        public override BaseModel GetDataFromJSON(string json)
        {
            return Utils.SafeFromJson <Model> (json);
        }
    }

    public DCLLockedOnEdit(IParcelScene scene) : base(scene)
    {
        model = new Model();
    }

    public override int GetClassId()
    {
        return (int) CLASS_ID.LOCKED_ON_EDIT;
    }

    public void SetIsLocked(bool value)
    {
        Model model = (Model)this.model;
        model.isLocked = value;
    }

    public override IEnumerator ApplyChanges(BaseModel baseModel)
    {
        RaiseOnAppliedChanges();
        return null;
    }
}