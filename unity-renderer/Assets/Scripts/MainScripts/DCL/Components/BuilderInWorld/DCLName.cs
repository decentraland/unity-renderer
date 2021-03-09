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
    public class Model : BaseModel
    {
        public string value;

        public override BaseModel GetDataFromJSON(string json)
        {
            return Utils.SafeFromJson<Model>(json);
        }
    }

    public DCLName(IParcelScene scene) : base(scene)
    {
        model = new Model();
    }

    private string oldName;

    public override int GetClassId()
    {
        return (int) CLASS_ID.NAME;
    }

    public override IEnumerator ApplyChanges(BaseModel newModel)
    {
        Model modelToApply = (Model)newModel;

        model = modelToApply;

        foreach (DecentralandEntity entity in attachedEntities)
        {
            entity.OnNameChange?.Invoke(modelToApply);
        }

#if UNITY_EDITOR
        foreach (DecentralandEntity decentralandEntity in this.attachedEntities)
        {
            if (!string.IsNullOrEmpty(oldName))
                decentralandEntity.gameObject.name.Replace(oldName, "");

            decentralandEntity.gameObject.name += $"-{modelToApply.value}";
        }
#endif
        oldName = modelToApply.value;
        return null;
    }

    public void SetNewName(string value)
    {
        Model newModel = new Model();
        newModel.value = value;
        UpdateFromModel(newModel);
    }
}