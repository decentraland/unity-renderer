using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    [Serializable]
    public class BuilderEntity
    {
        public string id;
        public List<string> components = new List<string>();
        public bool disableGizmos = false;
        public string name;
    }
}