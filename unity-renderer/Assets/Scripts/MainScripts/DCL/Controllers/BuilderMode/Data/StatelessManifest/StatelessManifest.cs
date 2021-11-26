using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public class StatelessManifest
    {
        public int schemaVersion = 1;
        public List<Entity> entities = new List<Entity>();
    }

    public class Entity
    {
        public string id;
        public List<Component> components = new List<Component>();
    }

    public class Component
    {
        public string type;
        public object value;
    }
}
