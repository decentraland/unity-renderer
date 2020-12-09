using DCL;
using DCL.Components;
using DCL.Controllers;
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

    public override IEnumerator ApplyChanges(string newJson)
    {
        Model newModel = SceneController.i.SafeFromJson<Model>(newJson);
        if (newModel.isLocked != model.isLocked)
        {
            model = newModel;
            RaiseOnAppliedChanges();
        }

        return null;
    }
}