using System;
using System.Collections;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;

namespace DCL
{
    public class UUIDComponent : BaseComponent
    {
        [Serializable]
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
                    case OnPointerHoverEnter.NAME:
                        return CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER;
                    case OnPointerHoverExit.NAME:
                        return CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT;
                }

                return CLASS_ID_COMPONENT.UUID_CALLBACK;
            }
        }

        public override string componentName => uuidComponentName;

        protected virtual string uuidComponentName { get; }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            this.model = newModel ?? new Model();
            return null;
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_CALLBACK;
        }
    }
}