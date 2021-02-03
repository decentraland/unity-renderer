using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
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

    public override int GetClassId()
    {
        return (int)CLASS_ID.NAME;
    }

    public override object GetModel()
    {
        return model;
    }

    public void SetNewName(string value)
    {
        Model newModel = new Model();
        newModel.value = value;

        UpdateFromJSON(JsonUtility.ToJson(newModel));
    }

    public void ForceSetNewName(string value)
    {
        Model newModel = new Model();
        newModel.value = value;

        ApplyNewValue(JsonUtility.ToJson(newModel));
    }

    public override IEnumerator ApplyChanges(string newJson)
    {
        ApplyNewValue(newJson);
        return null;
    }

    void ApplyNewValue(string newJson)
    {
        Model newModel = Utils.SafeFromJson<Model>(newJson);
        if (newModel.value != model.value)
        {
            string oldValue = model.value;
            model = newModel;

            foreach (DecentralandEntity entity in attachedEntities)
            {
                entity.OnNameChange?.Invoke(newModel);
            }

#if UNITY_EDITOR
            foreach (DecentralandEntity decentralandEntity in this.attachedEntities)
            {
                if (oldValue != null)
                    decentralandEntity.gameObject.name.Replace(oldValue, "");

                decentralandEntity.gameObject.name += $"-{model.value}"; 
            }
#endif
        }
    }
}