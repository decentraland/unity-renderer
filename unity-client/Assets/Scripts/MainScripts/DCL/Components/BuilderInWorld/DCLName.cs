using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component is a descriptive name of the Entity. In the BuilderInWorld you can give an entity a descriptive name through the entity list.
/// Builder in World send a message to kernel to change the value of this component in order to assign a descriptive name
/// </summary>
public class DCLName : BaseDisposable
{
    [System.Serializable]
    public class Model
    {
        public string value;
    }

    public Model model;

    public DCLName(ParcelScene scene) : base(scene)
    {
        model = new Model();
    }

    public override IEnumerator ApplyChanges(string newJson)
    {
        Model newModel = SceneController.i.SafeFromJson<Model>(newJson);
        if(newModel.value != model.value)
        {
            string oldValue = model.value;

            model = newModel;
            RaiseOnAppliedChanges();

#if UNITY_EDITOR
            foreach (DecentralandEntity decentralandEntity in this.attachedEntities)
            {
                decentralandEntity.gameObject.name.Replace(oldValue, "");
                decentralandEntity.gameObject.name += "-"+model.value;
            }
#endif
        }

        return null;
    }
}
