using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    [System.Serializable]
    public class SmartItemActionable
    {
        [NonSerialized]
        public string actionableId;
        public string entityId;
        public string actionId;
        public Dictionary<object, object> values = new Dictionary<object, object>();
    }

    [System.Serializable]
    public class SmartItemAction
    {
        public string id;
        public string label;
        public SmartItemParameter[] parameters;
    }

    [System.Serializable]
    public class SmartItemParameter
    {
        public enum ParameterType
        {
            BOOLEAN,
            TEXT,
            TEXTAREA,
            FLOAT,
            INTEGER,
            SLIDER,
            OPTIONS,
            ENTITY,
            ACTIONS
        }

        public string id;
        public string label;
        public string type;
        public string @default;
        public string min;
        public string max;
        public string step;
        public OptionsParameter[] options;

        private ParameterType enumType;
        private bool enumCached = false;

        [System.Serializable]
        public class OptionsParameter
        {
            public string label;
            public string value;
        }

        public ParameterType GetParameterType()
        {
            if (enumCached)
                return enumType;

            if (!Enum.TryParse(type.ToUpper(), out ParameterType myStatus))
                Debug.Log($"Error parsing the smart item parameter type: {type}, The parameter doesn't exist!");

            enumType = myStatus;
            enumCached = true;
            return enumType;
        }
    }
}