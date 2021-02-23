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
    public class Model
    {
        public bool isVisible;
    }

    public Model model;

    public DCLVisibleOnEdit(IParcelScene scene) : base(scene)
    {
        model = new Model();
    }

    public override int GetClassId()
    {
        return (int) CLASS_ID.VISIBLE_ON_EDIT;
    }

    public override object GetModel()
    {
        return model;
    }

    public override IEnumerator ApplyChanges(string newJson)
    {
        Model newModel = Utils.SafeFromJson<Model>(newJson);
        if (newModel.isVisible != model.isVisible)
        {
            model = newModel;
            RaiseOnAppliedChanges();
        }

        return null;
    }
}