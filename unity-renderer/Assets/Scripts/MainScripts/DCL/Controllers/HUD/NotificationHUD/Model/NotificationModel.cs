using System;

namespace DCL.NotificationModel
{
    public enum Type
    {
        GENERIC,
        SCRIPTING_ERROR,
        COMMS_ERROR,
        AIRDROPPING,
        GENERIC_WITHOUT_BUTTON,
        CUSTOM,
        UI_HIDDEN,
        GRAPHIC_CARD_WARNING,
        WARNING,
        CAMERA_MODE_LOCKED_BY_SCENE,
        WARNING_NO_ICON
    }

    public class Model
    {
        public Type type;
        public string message;
        public string buttonMessage;
        public float timer;
        public string scene;
        public Action callback;
        public string externalCallbackID;

        public string groupID;
        public bool destroyOnFinish = false;
    }
}