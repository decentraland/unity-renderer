using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
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
        public string id;
        public string label;
        public string type;
        public string @default;
        public string min;
        public string max;
        public string step;
        public OptionsParameter[] options;

        [System.Serializable]
        public class OptionsParameter
        {
            public string label;
            public string value;
        }
    }
}