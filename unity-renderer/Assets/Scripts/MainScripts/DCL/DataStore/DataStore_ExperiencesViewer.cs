using UnityEngine;

namespace DCL
{
    public class DataStore_ExperiencesViewer
    {
        public readonly BaseVariable<Transform> isInitialized = new BaseVariable<Transform>(null);
        public readonly BaseVariable<bool> isOpen = new BaseVariable<bool>(false);
    }
}