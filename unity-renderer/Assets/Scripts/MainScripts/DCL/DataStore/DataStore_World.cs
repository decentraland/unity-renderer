using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class DataStore_World
    {
        public readonly BaseHashSet<string> portableExperienceIds = new ();
        public readonly BaseDictionary<string, (string name, string description, string icon)> disabledPortableExperienceIds = new ();
        public readonly BaseVariable<GraphicRaycaster> currentRaycaster = new BaseVariable<GraphicRaycaster>();

        public BaseVariable<Transform> avatarTransform = new BaseVariable<Transform>(null);
        public BaseVariable<Transform> fpsTransform = new BaseVariable<Transform>(null);
        public BaseVariable<string> requestTeleportData = new BaseVariable<string>(null);
    }
}
