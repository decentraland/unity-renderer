using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    [Serializable]
    public class StatelessManifest
    {
        public int schemaVersion = 1;
        public List<Entity> entities = new List<Entity>();
    }

    [Serializable]
    public class Entity
    {
        public string id;
        public List<Component> components = new List<Component>();
    }

    [Serializable]
    public class Component
    {
        public string type;
        public object value;
    }
}