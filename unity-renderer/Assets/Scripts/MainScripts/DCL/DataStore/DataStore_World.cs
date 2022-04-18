using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class DataStore_Cursor
    {
        public enum CursorType
        {
            NORMAL,
            HOVER
        }

        public readonly BaseVariable<bool> visible = new BaseVariable<bool>(true);
        public readonly BaseVariable<CursorType> cursorType = new BaseVariable<CursorType>(CursorType.NORMAL);
    }

    public class DataStore_World
    {
        public readonly BaseCollection<string> portableExperienceIds = new BaseCollection<string>();
        public readonly BaseVariable<GraphicRaycaster> currentRaycaster = new BaseVariable<GraphicRaycaster>();
        
        public BaseVariable<Transform> avatarTransform = new BaseVariable<Transform>(null);
        public BaseVariable<Transform> fpsTransform = new BaseVariable<Transform>(null);
    }
}