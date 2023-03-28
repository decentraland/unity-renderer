using System;

namespace DCL.NotificationModel
{
    public enum Type
    {
        GENERIC,
        SCRIPTING_ERROR,
        COMMS_ERROR,
        [Obsolete("Deprecated behaviour")]
        AIRDROPPING,
        GENERIC_WITHOUT_BUTTON,
        CUSTOM,
        UI_HIDDEN,
        GRAPHIC_CARD_WARNING,
        WARNING,
        CAMERA_MODE_LOCKED_BY_SCENE,
        WARNING_NO_ICON,
        ERROR
    }

    public class Model
    {
        public Type type;
        public string message;
        public string buttonMessage;
        public float timer;
        public int scene;
        public Action callback;
        public string externalCallbackID;

        public string groupID;
        public bool destroyOnFinish = false;
    }
}
