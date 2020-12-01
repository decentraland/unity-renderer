using DCL;
using DCL.Components;
using DCL.Controllers;
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
        public bool value;
    }

    public Model model;

    public DCLVisibleOnEdit(ParcelScene scene) : base(scene)
    {
        model = new Model();
    }

    public override IEnumerator ApplyChanges(string newJson)
    {
        Model newModel = SceneController.i.SafeFromJson<Model>(newJson);
        if (newModel.value != model.value)
        {
            model = newModel;
            RaiseOnAppliedChanges();
        }

        return null;
    }
}