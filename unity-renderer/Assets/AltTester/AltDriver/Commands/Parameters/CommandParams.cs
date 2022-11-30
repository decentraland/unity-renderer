using System;
using Altom.AltDriver.Logging;
using Altom.AltDriver.Notifications;
using Newtonsoft.Json;
namespace Altom.AltDriver.Commands
{
    public class CommandParams
    {
        public string messageId;
        public string commandName;
        public CommandParams()
        {
            CommandAttribute cmdAttribute =
                (CommandAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(CommandAttribute));
            if (cmdAttribute == null)
                throw new Exception("No CommandAttribute found on type " + this.GetType());
            this.commandName = cmdAttribute.Name;
        }

        [JsonConstructor]
        public CommandParams(string commandName, string messageId)
        {
            this.commandName = commandName;
            this.messageId = messageId;
        }
    }

    public class CommandAttribute : Attribute
    {
        private string name;
        public CommandAttribute(string name)
        {
            this.name = name;
        }

        public string Name { get { return name; } }
    }

    public class CommandError
    {
        public string type;
        public string message;
        public string trace;
    }

    public class CommandResponse
    {
        public string messageId;
        public string commandName;
        public CommandError error;
        public String data;
        public bool isNotification;
    }

    public class BaseFindObjectsParams : CommandParams
    {
        public string path;
        public By cameraBy { get; protected set; }
        public string cameraPath { get; protected set; }
        public bool enabled { get; protected set; }

        public BaseFindObjectsParams(string path, By cameraBy, string cameraPath, bool enabled) : base()
        {
            this.path = path;
            this.cameraBy = cameraBy;
            this.cameraPath = cameraPath;
            this.enabled = enabled;
        }
    }

    [Command("findObjects")]
    public class AltFindObjectsParams : BaseFindObjectsParams
    {
        public AltFindObjectsParams(string path, By cameraBy, string cameraPath, bool enabled) : base(path, cameraBy, cameraPath, enabled)
        {

        }
    }
    [Command("findObject")]
    public class AltFindObjectParams : BaseFindObjectsParams
    {
        public AltFindObjectParams(string path, By cameraBy, string cameraPath, bool enabled) : base(path, cameraBy, cameraPath, enabled)
        {

        }
    }

    [Command("findObjectsLight")]
    public class AltFindObjectsLightParams : BaseFindObjectsParams
    {
        public AltFindObjectsLightParams(string path, By cameraBy, string cameraPath, bool enabled) : base(path, cameraBy, cameraPath, enabled)
        {

        }
    }
    [Command("getAllLoadedScenesAndObjects")]
    public class AltGetAllLoadedScenesAndObjectsParams : BaseFindObjectsParams
    {
        public AltGetAllLoadedScenesAndObjectsParams(string path, By cameraBy, string cameraPath, bool enabled) : base(path, cameraBy, cameraPath, enabled)
        {

        }
    }

    [Command("getServerVersion")]
    public class AltGetServerVersionParams : CommandParams
    {
        public AltGetServerVersionParams() : base()
        {
        }
    }

    [Command("moveMouse")]
    public class AltMoveMouseParams : CommandParams
    {
        public AltVector2 coordinates;
        public float duration;
        public bool wait;

        public AltMoveMouseParams(AltVector2 coordinates, float duration, bool wait) : base()
        {
            this.coordinates = coordinates;
            this.duration = duration;
            this.wait = wait;
        }
    }

    [Command("multipointSwipe")]
    public class AltMultipointSwipeParams : CommandParams
    {
        public AltVector2[] positions;
        public float duration;
        public bool wait;

        public AltMultipointSwipeParams(AltVector2[] positions, float duration, bool wait) : base()
        {
            this.positions = positions;
            this.duration = duration;
            this.wait = wait;
        }
    }

    [Command("pressKeyboardKeys")]
    public class AltPressKeyboardKeysParams : CommandParams
    {
        public AltKeyCode[] keyCodes;
        public float power;
        public float duration;
        public bool wait;

        public AltPressKeyboardKeysParams(AltKeyCode[] keyCodes, float power, float duration, bool wait) : base()
        {
            this.keyCodes = keyCodes;
            this.power = power;
            this.duration = duration;
            this.wait = wait;
        }
    }


    [Command("scroll")]
    public class AltScrollParams : CommandParams
    {
        public float speed;//TODO change to vector2
        public float speedHorizontal;
        public float duration;
        public bool wait;

        public AltScrollParams(float speed, float duration, bool wait, float speedHorizontal = 0) : base()
        {
            this.speed = speed;
            this.duration = duration;
            this.wait = wait;
            this.speedHorizontal = speedHorizontal;
        }
    }

    [Command("swipe")]
    public class AltSwipeParams : CommandParams
    {
        public AltVector2 start;
        public AltVector2 end;
        public float duration;
        public bool wait;

        public AltSwipeParams(AltVector2 start, AltVector2 end, float duration, bool wait) : base()
        {
            this.start = start;
            this.end = end;
            this.duration = duration;
            this.wait = wait;
        }
    }


    [Command("tilt")]
    public class AltTiltParams : CommandParams
    {
        public AltVector3 acceleration;
        public float duration;
        public bool wait;

        public AltTiltParams(AltVector3 acceleration, float duration, bool wait) : base()
        {
            this.acceleration = acceleration;
            this.duration = duration;
            this.wait = wait;
        }
    }
    public class BaseAltObjectParams : CommandParams
    {
        public AltObject altObject;
        public BaseAltObjectParams(AltObject altObject) : base()
        {
            this.altObject = altObject;
        }
    }

    [Command("callComponentMethodForObject")]
    public class AltCallComponentMethodForObjectParams : BaseAltObjectParams
    {
        public string component;
        public string method;
        public string[] parameters;
        public string[] typeOfParameters;
        public string assembly;


        public AltCallComponentMethodForObjectParams(AltObject altObject, string component, string method, string[] parameters, string[] typeOfParameters, string assembly) : base(altObject)
        {
            this.component = component;
            this.method = method;
            this.parameters = parameters;
            this.typeOfParameters = typeOfParameters;
            this.assembly = assembly;
        }
    }

    [Command("getObjectComponentProperty")]
    public class AltGetObjectComponentPropertyParams : BaseAltObjectParams
    {
        public string component;
        public string property;
        public string assembly;
        public int maxDepth;

        public AltGetObjectComponentPropertyParams(AltObject altObject, string component, string property, string assembly, int maxDepth) : base(altObject)
        {
            this.component = component;
            this.property = property;
            this.assembly = assembly;
            this.maxDepth = maxDepth;
        }
    }
    [Command("setObjectComponentProperty")]
    public class AltSetObjectComponentPropertyParams : BaseAltObjectParams
    {
        public string component;
        public string property;
        public string assembly;
        public string value;

        public AltSetObjectComponentPropertyParams(AltObject altObject, string component, string property, string assembly, string value) : base(altObject)
        {
            this.component = component;
            this.property = property;
            this.assembly = assembly;
            this.value = value;
        }
    }


    [Command("dragObject")]
    public class AltDragObjectParams : BaseAltObjectParams
    {
        public AltVector2 position;
        public AltDragObjectParams(AltObject altObject, AltVector2 position) : base(altObject)
        {
            this.altObject = altObject;
            this.position = position;
        }
    }
    [Command("getAllComponents")]
    public class AltGetAllComponentsParams : CommandParams
    {
        public int altObjectId;
        public AltGetAllComponentsParams(int altObjectId) : base()
        {
            this.altObjectId = altObjectId;
        }
    }

    [Command("getAllFields")]
    public class AltGetAllFieldsParams : CommandParams
    {
        public int altObjectId;
        public AltComponent altComponent;
        public AltFieldsSelections altFieldsSelections;

        public AltGetAllFieldsParams(int altObjectId, AltComponent altComponent, AltFieldsSelections altFieldsSelections) : base()
        {
            this.altObjectId = altObjectId;
            this.altComponent = altComponent;
            this.altFieldsSelections = altFieldsSelections;
        }
    }
    [Command("getAllProperties")]
    public class AltGetAllPropertiesParams : CommandParams
    {
        public int altObjectId;
        public AltComponent altComponent;
        public AltPropertiesSelections altPropertiesSelections;

        public AltGetAllPropertiesParams(int altObjectId, AltComponent altComponent, AltPropertiesSelections altPropertiesSelections) : base()
        {
            this.altObjectId = altObjectId;
            this.altComponent = altComponent;
            this.altPropertiesSelections = altPropertiesSelections;
        }
    }
    [Command("getAllMethods")]
    public class AltGetAllMethodsParams : CommandParams
    {
        public AltComponent altComponent;
        public AltMethodSelection methodSelection;

        public AltGetAllMethodsParams(AltComponent altComponent, AltMethodSelection methodSelection) : base()
        {
            this.altComponent = altComponent;
            this.methodSelection = methodSelection;
        }
    }
    [Command("getText")]
    public class AltGetTextParams : BaseAltObjectParams
    {
        public AltGetTextParams(AltObject altObject) : base(altObject)
        {
        }
    }

    [Command("setText")]
    public class AltSetTextParams : BaseAltObjectParams
    {
        public string value;
        public bool submit;

        public AltSetTextParams(AltObject altObject, string value, bool submit) : base(altObject)
        {
            this.value = value;
            this.submit = submit;
        }
    }

    [Command("pointerDownFromObject")]
    public class AltPointerDownFromObjectParams : BaseAltObjectParams
    {
        public AltPointerDownFromObjectParams(AltObject altObject) : base(altObject)
        {
        }
    }

    [Command("pointerEnterObject")]
    public class AltPointerEnterObjectParams : BaseAltObjectParams
    {
        public AltPointerEnterObjectParams(AltObject altObject) : base(altObject)
        {
        }
    }
    [Command("pointerExitObject")]
    public class AltPointerExitObjectParams : BaseAltObjectParams
    {
        public AltPointerExitObjectParams(AltObject altObject) : base(altObject)
        {
        }
    }
    [Command("pointerUpFromObject")]
    public class AltPointerUpFromObjectParams : BaseAltObjectParams
    {
        public AltPointerUpFromObjectParams(AltObject altObject) : base(altObject)
        {
        }
    }


    [Command("getPNGScreenshot")]
    public class AltGetPNGScreenshotParams : CommandParams
    {
        public AltGetPNGScreenshotParams() : base()
        {
        }
    }

    [Command("getScreenshot")]
    public class AltGetScreenshotParams : CommandParams
    {
        public AltVector2 size;
        public int quality;
        public AltGetScreenshotParams(AltVector2 size, int quality) : base()
        {
            this.size = size;
            this.quality = quality;
        }
    }

    [Command("hightlightObjectScreenshot")]
    public class AltHighlightObjectScreenshotParams : CommandParams
    {
        public int altObjectId;
        public AltColor color;
        public float width;
        public AltVector2 size;
        public int quality;
        public AltHighlightObjectScreenshotParams(int altObjectId, AltColor color, float width, AltVector2 size, int quality) : base()
        {
            this.altObjectId = altObjectId;
            this.color = color;
            this.width = width;
            this.size = size;
            this.quality = quality;
        }
    }

    [Command("hightlightObjectFromCoordinatesScreenshot")]
    public class AltHighlightObjectFromCoordinatesScreenshotParams : CommandParams
    {
        public AltVector2 coordinates;
        public AltColor color;
        public float width;
        public AltVector2 size;
        public int quality;
        public AltHighlightObjectFromCoordinatesScreenshotParams(AltVector2 coordinates, AltColor color, float width, AltVector2 size, int quality) : base()
        {
            this.coordinates = coordinates;
            this.color = color;
            this.width = width;
            this.size = size;
            this.quality = quality;
        }
    }

    [Command("deleteKeyPlayerPref")]
    public class AltDeleteKeyPlayerPrefParams : CommandParams
    {
        public string keyName;
        public AltDeleteKeyPlayerPrefParams(string keyName) : base()
        {
            this.keyName = keyName;
        }
    }
    [Command("getKeyPlayerPref")]
    public class AltGetKeyPlayerPrefParams : CommandParams
    {
        public string keyName;
        public PlayerPrefKeyType keyType;
        public AltGetKeyPlayerPrefParams() : base() { }
        public AltGetKeyPlayerPrefParams(string keyName, PlayerPrefKeyType keyType) : base()
        {
            this.keyName = keyName;
            this.keyType = keyType;
        }
    }

    [Command("setKeyPlayerPref")]
    public class AltSetKeyPlayerPrefParams : CommandParams
    {
        public string keyName;
        public PlayerPrefKeyType keyType;
        public string stringValue;
        public float floatValue;
        public int intValue;

        public AltSetKeyPlayerPrefParams() : base() { }

        public AltSetKeyPlayerPrefParams(string keyName, int value) : base()
        {
            this.keyName = keyName;
            this.intValue = value;
            keyType = PlayerPrefKeyType.Int;
        }
        public AltSetKeyPlayerPrefParams(string keyName, float value) : base()
        {
            this.keyName = keyName;
            this.floatValue = value;
            keyType = PlayerPrefKeyType.Float;
        }
        public AltSetKeyPlayerPrefParams(string keyName, string value) : base()
        {
            this.keyName = keyName;
            this.stringValue = value;
            keyType = PlayerPrefKeyType.String;
        }
    }

    [Command("deletePlayerPref")]
    public class AltDeletePlayerPrefParams : CommandParams
    {
        public AltDeletePlayerPrefParams() : base()
        {
        }
    }

    [Command("getAllActiveCameras")]
    public class AltGetAllActiveCamerasParams : CommandParams
    {
        public AltGetAllActiveCamerasParams() : base()
        {
        }
    }
    [Command("getAllCameras")]
    public class AltGetAllCamerasParams : CommandParams
    {

    }

    [Command("getAllLoadedScenes")]
    public class AltGetAllLoadedScenesParams : CommandParams
    {

    }
    [Command("getAllScenes")]
    public class AltGetAllScenesParams : CommandParams
    {

    }
    [Command("getCurrentScene")]
    public class AltGetCurrentSceneParams : CommandParams
    {

    }
    [Command("loadScene")]
    public class AltLoadSceneParams : CommandParams
    {
        public string sceneName;
        public bool loadSingle;

        public AltLoadSceneParams(string sceneName, bool loadSingle) : base()
        {
            this.sceneName = sceneName;
            this.loadSingle = loadSingle;
        }

    }

    [Command("unloadScene")]
    public class AltUnloadSceneParams : CommandParams
    {
        public string sceneName;

        public AltUnloadSceneParams(string sceneName) : base()
        {
            this.sceneName = sceneName;
        }

    }
    [Command("getTimeScale")]
    public class AltGetTimeScaleParams : CommandParams
    {

    }
    [Command("setTimeScale")]
    public class AltSetTimeScaleParams : CommandParams
    {
        public float timeScale;
        public AltSetTimeScaleParams(float timeScale) : base()
        {
            this.timeScale = timeScale;
        }
    }
    [Command("setServerLogging")]
    public class AltSetServerLoggingParams : CommandParams
    {
        public AltLogger logger;
        public AltLogLevel logLevel;

        public AltSetServerLoggingParams(AltLogger logger, AltLogLevel logLevel) : base()
        {
            this.logger = logger;
            this.logLevel = logLevel;
        }
    }

    [Command("tapElement")]
    public class AltTapElementParams : BaseAltObjectParams
    {
        public int count;
        public float interval;
        public bool wait;
        public AltTapElementParams(AltObject altObject, int count, float interval, bool wait) : base(altObject)
        {
            this.count = count;
            this.interval = interval;
            this.wait = wait;
        }
    }

    [Command("clickElement")]
    public class AltClickElementParams : BaseAltObjectParams
    {
        public int count;
        public float interval;
        public bool wait;
        public AltClickElementParams(AltObject altObject, int count, float interval, bool wait) : base(altObject)
        {
            this.count = count;
            this.interval = interval;
            this.wait = wait;
        }
    }

    [Command("clickCoordinates")]
    public class AltClickCoordinatesParams : CommandParams
    {
        public AltVector2 coordinates;
        public int count;
        public float interval;
        public bool wait;
        public AltClickCoordinatesParams(AltVector2 coordinates, int count, float interval, bool wait)
        {
            this.coordinates = coordinates;
            this.count = count;
            this.interval = interval;
            this.wait = wait;
        }
    }
    [Command("tapCoordinates")]
    public class AltTapCoordinatesParams : CommandParams
    {
        public AltVector2 coordinates;
        public int count;
        public float interval;
        public bool wait;
        public AltTapCoordinatesParams(AltVector2 coordinates, int count, float interval, bool wait)
        {
            this.coordinates = coordinates;
            this.count = count;
            this.interval = interval;
            this.wait = wait;
        }
    }

    [Command("keysUp")]
    public class AltKeysUpParams : CommandParams
    {
        public AltKeyCode[] keyCodes;

        public AltKeysUpParams(AltKeyCode[] keyCodes)
        {
            this.keyCodes = keyCodes;
        }
    }

    [Command("keysDown")]
    public class AltKeysDownParams : CommandParams
    {
        public AltKeyCode[] keyCodes;
        public float power;

        public AltKeysDownParams(AltKeyCode[] keyCodes, float power)
        {
            this.keyCodes = keyCodes;
            this.power = power;
        }
    }

    [Command("beginTouch")]
    public class AltBeginTouchParams : CommandParams
    {
        public AltVector2 coordinates;
        public AltBeginTouchParams(AltVector2 coordinates)
        {
            this.coordinates = coordinates;
        }
    }

    [Command("moveTouch")]
    public class AltMoveTouchParams : CommandParams
    {
        public int fingerId;
        public AltVector2 coordinates;
        public AltMoveTouchParams(int fingerId, AltVector2 coordinates)
        {
            this.coordinates = coordinates;
            this.fingerId = fingerId;
        }
    }

    [Command("endTouch")]
    public class AltEndTouchParams : CommandParams
    {
        public int fingerId;
        public AltEndTouchParams(int fingerId)
        {
            this.fingerId = fingerId;
        }
    }
    [Command("activateNotification")]
    public class ActivateNotification : CommandParams
    {
        public NotificationType NotificationType;

        public ActivateNotification(NotificationType notificationType)
        {
            NotificationType = notificationType;
        }
    }
    [Command("deactivateNotification")]
    public class DeactivateNotification : CommandParams
    {
        public NotificationType NotificationType;

        public DeactivateNotification(NotificationType notificationType)
        {
            NotificationType = notificationType;
        }
    }

    [Command("findObjectAtCoordinates")]
    public class AltFindObjectAtCoordinatesParams : CommandParams
    {
        public AltVector2 coordinates;
        public AltFindObjectAtCoordinatesParams(AltVector2 coordinates)
        {
            this.coordinates = coordinates;
        }
    }
}