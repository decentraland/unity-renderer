using UnityEngine;

namespace DCL
{
    public class DataStore_World
    {
        public BaseVariable<Transform> avatarTransform = new BaseVariable<Transform>(null);
        public BaseVariable<Transform> fpsTransform = new BaseVariable<Transform>(null);
    }
}