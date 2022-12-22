using UnityEngine;

namespace DCL
{
    public class DataStore_Outliner
    {
        public readonly BaseVariable<(Renderer renderer, int meshCount, float avatarHeight)> avatarOutlined = new ();
    }
}
