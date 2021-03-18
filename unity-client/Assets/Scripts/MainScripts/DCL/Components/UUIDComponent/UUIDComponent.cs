using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class UUIDComponent : BaseComponent
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public string type;
            public string uuid;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public CLASS_ID_COMPONENT GetClassIdFromType()
            {
                switch (type)
                {
                    case OnPointerDown.NAME:
                        return CLASS_ID_COMPONENT.UUID_ON_DOWN;
                    case OnPointerUp.NAME:
                        return CLASS_ID_COMPONENT.UUID_ON_UP;
                    case OnClick.NAME:
                        return CLASS_ID_COMPONENT.UUID_ON_CLICK;
                }

                return CLASS_ID_COMPONENT.UUID_CALLBACK;
            }
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            this.model = newModel ?? new UUIDComponent.Model();
            return null;
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_CALLBACK;
        }
    }
}