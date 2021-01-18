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
    public class Model
    {
        public bool isLocked;
    }

    public Model model;

    public DCLLockedOnEdit(ParcelScene scene) : base(scene)
    {
        model = new Model();
    }
    public override int GetClassId()
    {
        return (int)CLASS_ID.LOCKED_ON_EDIT;
    }

    public override object GetModel()
    {
        return model;
    }

    public void SetIsLocked(bool value)
    {
        model.isLocked = value;
    }

    public override IEnumerator ApplyChanges(string newJson)
    {
        Model newModel = Utils.SafeFromJson<Model>(newJson);
        if (newModel.isLocked != model.isLocked)
        {
            model = newModel;
            RaiseOnAppliedChanges();
        }

        return null;
    }
}