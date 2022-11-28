using System;
using System.Collections.Generic;
using System.Threading;
using Altom.AltDriver.Commands;
using Altom.AltDriver.Logging;
using Altom.AltDriver.Notifications;

namespace Altom.AltDriver
{
    public enum By
    {
        TAG, LAYER, NAME, COMPONENT, PATH, ID, TEXT
    }

    public class AltDriver
    {
        private static readonly NLog.Logger logger = DriverLogManager.Instance.GetCurrentClassLogger();
        private readonly IDriverCommunication communicationHandler;
        public static readonly string VERSION = "1.8.0";

        public IDriverCommunication CommunicationHandler { get { return communicationHandler; } }

        /// <summary>
        /// Initiates AltDriver and begins connection with the instrumented Unity application through to AltProxy
        /// </summary>
        /// <param name="host">The ip or hostname  AltProxy is listening on.</param>
        /// <param name="port">The port AltProxy is listening on.</param>
        /// <param name="enableLogging">If true it enables driver commands logging to log file and Unity.</param>
        /// <param name="connectTimeout">The connect timeout in seconds.</param>
        public AltDriver(string host = "127.0.0.1", int port = 13000, bool enableLogging = false, int connectTimeout = 60, string gameName = "__default__")
        {
#if UNITY_EDITOR || ALTTESTER
            var defaultLevels = new Dictionary<AltLogger, AltLogLevel> { { AltLogger.File, AltLogLevel.Debug }, { AltLogger.Unity, AltLogLevel.Debug } };
#else
                var defaultLevels = new Dictionary<AltLogger, AltLogLevel> { { AltLogger.File, AltLogLevel.Debug }, { AltLogger.Console, AltLogLevel.Debug } };
#endif

            DriverLogManager.SetupAltDriverLogging(defaultLevels);

            if (!enableLogging)
            {
                DriverLogManager.StopLogging();
            }

            logger.Debug(
                "Connecting to AltTester on host: '{0}', port: '{1}' and gameName: '{2}'.",
                host,
                port,
                gameName
            );
            communicationHandler = new DriverCommunicationWebSocket(host, port, connectTimeout, gameName);
            communicationHandler.Connect();

            checkServerVersion();
        }

        private void splitVersion(string version, out string major, out string minor)
        {
            var parts = version.Split(new[] { "." }, StringSplitOptions.None);
            major = parts[0];
            minor = parts.Length > 1 ? parts[1] : string.Empty;
        }

        private void checkServerVersion()
        {
            string serverVersion = GetServerVersion();

            string majorServer;
            string majorDriver;
            string minorDriver;
            string minorServer;

            splitVersion(serverVersion, out majorServer, out minorServer);
            splitVersion(VERSION, out majorDriver, out minorDriver);

            if (majorServer != majorDriver || minorServer != minorDriver)
            {
                string message = "Version mismatch. AltDriver version is " + VERSION + ". AltTester version is " + serverVersion + ".";
                logger.Warn(message);
            }
        }

        public void Stop()
        {
            communicationHandler.Close();
        }

        public void SetCommandResponseTimeout(int commandTimeout)
        {
            communicationHandler.SetCommandTimeout(commandTimeout);
        }

        public void SetDelayAfterCommand(float delay)
        {
            communicationHandler.SetDelayAfterCommand(delay);
        }

        public float GetDelayAfterCommand()
        {
            return communicationHandler.GetDelayAfterCommand();
        }

        public string GetServerVersion()
        {
            string serverVersion = new AltGetServerVersion(communicationHandler).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return serverVersion;
        }

        public void SetLogging(bool enableLogging)
        {
            if (enableLogging)
                DriverLogManager.ResumeLogging();
            else
                DriverLogManager.StopLogging();
        }

        public void LoadScene(string scene, bool loadSingle = true)
        {
            new AltLoadScene(communicationHandler, scene, loadSingle).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public void UnloadScene(string scene)
        {
            new AltUnloadScene(communicationHandler, scene).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public List<string> GetAllLoadedScenes()
        {
            var sceneList = new AltGetAllLoadedScenes(communicationHandler).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return sceneList;
        }

        public List<AltObject> FindObjects(By by, string value, By cameraBy = By.NAME, string cameraValue = "", bool enabled = true)
        {
            var listOfObjects = new AltFindObjects(communicationHandler, by, value, cameraBy, cameraValue, enabled).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return listOfObjects;
        }

        public List<AltObject> FindObjectsWhichContain(By by, string value, By cameraBy = By.NAME, string cameraValue = "", bool enabled = true)
        {
            var listOfObjects = new AltFindObjectsWhichContain(communicationHandler, by, value, cameraBy, cameraValue, enabled).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return listOfObjects;
        }

        public AltObject FindObject(By by, string value, By cameraBy = By.NAME, string cameraValue = "", bool enabled = true)
        {
            var findObject = new AltFindObject(communicationHandler, by, value, cameraBy, cameraValue, enabled).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return findObject;
        }

        public AltObject FindObjectWhichContains(By by, string value, By cameraBy = By.NAME, string cameraValue = "", bool enabled = true)
        {
            var findObject = new AltFindObjectWhichContains(communicationHandler, by, value, cameraBy, cameraValue, enabled).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return findObject;
        }

        public void SetTimeScale(float timeScale)
        {
            new AltSetTimeScale(communicationHandler, timeScale).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public float GetTimeScale()
        {
            var timeScale = new AltGetTimeScale(communicationHandler).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return timeScale;
        }

        public T CallStaticMethod<T>(string typeName, string methodName, string assemblyName,
                    object[] parameters, string[] typeOfParameters = null)
        {
            var result = new AltCallStaticMethod<T>(communicationHandler, typeName, methodName, parameters, typeOfParameters, assemblyName).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return result;
        }

        public T GetStaticProperty<T>(string componentName, string propertyName, string assemblyName, int maxDepth = 2)
        {
            var propertyValue = new AltGetStaticProperty<T>(communicationHandler, componentName, propertyName, assemblyName, maxDepth).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return propertyValue;
        }

        public void DeletePlayerPref()
        {
            new AltDeletePlayerPref(communicationHandler).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public void DeleteKeyPlayerPref(string keyName)
        {
            new AltDeleteKeyPlayerPref(communicationHandler, keyName).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public void SetKeyPlayerPref(string keyName, int valueName)
        {
            new AltSetKeyPLayerPref(communicationHandler, keyName, valueName).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public void SetKeyPlayerPref(string keyName, float valueName)
        {
            new AltSetKeyPLayerPref(communicationHandler, keyName, valueName).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public void SetKeyPlayerPref(string keyName, string valueName)
        {
            new AltSetKeyPLayerPref(communicationHandler, keyName, valueName).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public int GetIntKeyPlayerPref(string keyName)
        {
            var keyValue = new AltGetIntKeyPlayerPref(communicationHandler, keyName).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return keyValue;
        }

        public float GetFloatKeyPlayerPref(string keyName)
        {
            var keyValue = new AltGetFloatKeyPlayerPref(communicationHandler, keyName).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return keyValue;
        }

        public string GetStringKeyPlayerPref(string keyName)
        {
            var keyValue = new AltGetStringKeyPlayerPref(communicationHandler, keyName).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return keyValue;
        }

        public string GetCurrentScene()
        {
            var sceneName = new AltGetCurrentScene(communicationHandler).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return sceneName;
        }

        /// <summary>
        /// Simulates a swipe action between two points.
        /// </summary>
        /// <param name="start">Coordinates of the screen where the swipe begins</param>
        /// <param name="end">Coordinates of the screen where the swipe ends</param>
        /// <param name="duration">The time measured in seconds to move the mouse from start to end location. Defaults to <c>0.1</c>.</param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void Swipe(AltVector2 start, AltVector2 end, float duration = 0.1f, bool wait = true)
        {
            new AltSwipe(communicationHandler, start, end, duration, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Simulates a multipoint swipe action.
        /// </summary>
        /// <param name="positions">A list of positions on the screen where the swipe be made.</param>
        /// <param name="duration">The time measured in seconds to swipe from first position to the last position. Defaults to <code>0.1</code>.</param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void MultipointSwipe(AltVector2[] positions, float duration = 0.1f, bool wait = true)
        {
            new AltMultipointSwipe(communicationHandler, positions, duration, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Simulates holding left click button down for a specified amount of time at given coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates where the button is held down.</param>
        /// <param name="duration">The time measured in seconds to keep the button down.</param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void HoldButton(AltVector2 coordinates, float duration, bool wait = true)
        {
            Swipe(coordinates, coordinates, duration, wait);
        }

        /// <summary>
        /// Simulates key press action in your game.
        /// </summary>
        /// <param name="keyCode">The key code of the key simulated to be pressed.</param>
        /// <param name="power" >A value between [-1,1] used for joysticks to indicate how hard the button was pressed. Defaults to <c>1</c>.</param>
        /// <param name="duration">The time measured in seconds from the key press to the key release. Defaults to <c>0.1</c></param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void PressKey(AltKeyCode keyCode, float power = 1, float duration = 0.1f, bool wait = true)
        {
            AltKeyCode[] keyCodes = { keyCode };
            PressKeys(keyCodes, power, duration, wait);
        }

        /// <summary>
        /// Simulates multiple keys pressed action in your game.
        /// </summary>
        /// <param name="keyCodes">The list of key codes of the keys simulated to be pressed.</param>
        /// <param name="power" >A value between [-1,1] used for joysticks to indicate how hard the button was pressed. Defaults to <c>1</c>.</param>
        /// <param name="duration">The time measured in seconds from the key press to the key release. Defaults to <c>0.1</c></param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void PressKeys(AltKeyCode[] keyCodes, float power = 1, float duration = 0.1f, bool wait = true)
        {
            new AltPressKeys(communicationHandler, keyCodes, power, duration, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public void KeyDown(AltKeyCode keyCode, float power = 1)
        {
            AltKeyCode[] keyCodes = { keyCode };
            KeysDown(keyCodes, power);
        }

        /// <summary>
        /// Simulates multiple keys down action in your game.
        /// </summary>
        /// <param name="keyCodes">The key codes of the keys simulated to be down.</param>
        /// <param name="power" >A value between [-1,1] used for joysticks to indicate how hard the button was pressed. Defaults to <c>1</c>.</param>
        public void KeysDown(AltKeyCode[] keyCodes, float power = 1)
        {
            new AltKeysDown(communicationHandler, keyCodes, power).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public void KeyUp(AltKeyCode keyCode)
        {
            AltKeyCode[] keyCodes = { keyCode };
            KeysUp(keyCodes);
        }

        /// <summary>
        /// Simulates multiple keys up action in your game.
        /// </summary>
        /// <param name="keyCodes">The key codes of the keys simulated to be up.</param>
        public void KeysUp(AltKeyCode[] keyCodes)
        {
            new AltKeysUp(communicationHandler, keyCodes).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Simulate mouse movement in your game.
        /// </summary>
        /// <param name="coordinates">The screen coordinates</param>
        /// <param name="duration">The time measured in seconds to move the mouse from the current mouse position to the set coordinates. Defaults to <c>0.1f</c></param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void MoveMouse(AltVector2 coordinates, float duration = 0.1f, bool wait = true)
        {
            new AltMoveMouse(communicationHandler, coordinates, duration, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Simulate scroll action in your game.
        /// </summary>
        /// <param name="speed">Set how fast to scroll. Positive values will scroll up and negative values will scroll down. Defaults to <code> 1 </code></param>
        /// <param name="duration">The duration of the scroll in seconds. Defaults to <code> 0.1 </code></param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void Scroll(float speed = 1, float duration = 0.1f, bool wait = true)
        {
            new AltScroll(communicationHandler, speed, 0, duration, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Simulate scroll action in your game.
        /// </summary>
        /// <param name="scrollValue">Set how fast to scroll. X is horizontal and Y is vertical. Defaults to <code> 1 </code></param>
        /// <param name="duration">The duration of the scroll in seconds. Defaults to <code> 0.1 </code></param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void Scroll(AltVector2 scrollValue, float duration = 0.1f, bool wait = true)
        {
            new AltScroll(communicationHandler, scrollValue.y, scrollValue.x, duration, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Tap at screen coordinates
        /// </summary>
        /// <param name="coordinates">The screen coordinates</param>
        /// <param name="count">Number of taps</param>
        /// <param name="interval">Interval between taps in seconds</param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void Tap(AltVector2 coordinates, int count = 1, float interval = 0.1f, bool wait = true)
        {
            new AltTapCoordinates(communicationHandler, coordinates, count, interval, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Click at screen coordinates
        /// </summary>
        /// <param name="coordinates">The screen coordinates</param>
        /// <param name="count" >Number of clicks.</param>
        /// <param name="interval">Interval between clicks in seconds</param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void Click(AltVector2 coordinates, int count = 1, float interval = 0.1f, bool wait = true)
        {
            new AltClickCoordinates(communicationHandler, coordinates, count, interval, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Simulates device rotation action in your game.
        /// </summary>
        /// <param name="acceleration">The linear acceleration of a device.</param>
        /// <param name="duration">How long the rotation will take in seconds. Defaults to <code>0.1<code>.</param>
        /// <param name="wait">If set wait for command to finish. Defaults to <c>True</c>.</param>
        public void Tilt(AltVector3 acceleration, float duration = 0.1f, bool wait = true)
        {
            new AltTilt(communicationHandler, acceleration, duration, wait).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public List<AltObject> GetAllElements(By cameraBy = By.NAME, string cameraValue = "", bool enabled = true)
        {
            var listOfObjects = new AltGetAllElements(communicationHandler, cameraBy, cameraValue, enabled).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return listOfObjects;
        }

        public List<AltObjectLight> GetAllElementsLight(By cameraBy = By.NAME, string cameraValue = "", bool enabled = true)
        {
            var listOfObjects = new AltGetAllElementsLight(communicationHandler, cameraBy, cameraValue, enabled).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return listOfObjects;
        }

        public void WaitForCurrentSceneToBe(string sceneName, double timeout = 10, double interval = 1)
        {
            new AltWaitForCurrentSceneToBe(communicationHandler, sceneName, timeout, interval).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public AltObject WaitForObject(By by, string value, By cameraBy = By.NAME, string cameraValue = "", bool enabled = true, double timeout = 20, double interval = 0.5)
        {
            var objectFound = new AltWaitForObject(communicationHandler, by, value, cameraBy, cameraValue, enabled, timeout, interval).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return objectFound;
        }

        public void WaitForObjectNotBePresent(By by, string value, By cameraBy = By.NAME, string cameraValue = "", bool enabled = true, double timeout = 20, double interval = 0.5)
        {
            new AltWaitForObjectNotBePresent(communicationHandler, by, value, cameraBy, cameraValue, enabled, timeout, interval).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public AltObject WaitForObjectWhichContains(By by, string value, By cameraBy = By.NAME, string cameraValue = "", bool enabled = true, double timeout = 20, double interval = 0.5)
        {
            var objectFound = new AltWaitForObjectWhichContains(communicationHandler, by, value, cameraBy, cameraValue, enabled, timeout, interval).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return objectFound;
        }

        public List<string> GetAllScenes()
        {
            var listOfScenes = new AltGetAllScenes(communicationHandler).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return listOfScenes;
        }

        public List<AltObject> GetAllCameras()
        {
            var listOfCameras = new AltGetAllCameras(communicationHandler).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return listOfCameras;
        }

        public List<AltObject> GetAllActiveCameras()
        {
            var listOfCameras = new AltGetAllActiveCameras(communicationHandler).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return listOfCameras;
        }

        public AltTextureInformation GetScreenshot(AltVector2 size = default(AltVector2), int screenShotQuality = 100)
        {
            var textureInformation = new AltGetScreenshot(communicationHandler, size, screenShotQuality).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return textureInformation;
        }

        public AltTextureInformation GetScreenshot(int id, AltColor color, float width, AltVector2 size = default(AltVector2), int screenShotQuality = 100)
        {
            var textureInformation = new AltGetHighlightObjectScreenshot(communicationHandler, id, color, width, size, screenShotQuality).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return textureInformation;
        }

        public AltTextureInformation GetScreenshot(AltVector2 coordinates, AltColor color, float width, out AltObject selectedObject, AltVector2 size = default(AltVector2), int screenShotQuality = 100)
        {
            var textureInformation = new AltGetHighlightObjectFromCoordinatesScreenshot(communicationHandler, coordinates, color, width, size, screenShotQuality).Execute(out selectedObject);
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return textureInformation;
        }

        public void GetPNGScreenshot(string path)
        {
            new AltGetPNGScreenshot(communicationHandler, path).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public List<AltObjectLight> GetAllLoadedScenesAndObjects(bool enabled = true)
        {
            var listOfObjects = new AltGetAllLoadedScenesAndObjects(communicationHandler, enabled).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return listOfObjects;
        }

        public void SetServerLogging(AltLogger logger, AltLogLevel logLevel)
        {
            new AltSetServerLogging(communicationHandler, logger, logLevel).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public int BeginTouch(AltVector2 screenPosition)
        {
            var touchId = new AltBeginTouch(communicationHandler, screenPosition).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return touchId;
        }

        public void MoveTouch(int fingerId, AltVector2 screenPosition)
        {
            new AltMoveTouch(communicationHandler, fingerId, screenPosition).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        public void EndTouch(int fingerId)
        {
            new AltEndTouch(communicationHandler, fingerId).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
        }

        /// <summary>
        /// Retrieves the Unity object at given coordinates.
        /// Uses EventSystem.RaycastAll to find object. If no object is found then it uses UnityEngine.Physics.Raycast and UnityEngine.Physics2D.Raycast and returns the one closer to the camera.
        /// </summary>
        /// <param name="coordinates">The screen coordinates</param>
        /// <returns>The UI object hit by event system Raycast, null otherwise</returns>
        public AltObject FindObjectAtCoordinates(AltVector2 coordinates)
        {
            var objectFound = new AltFindObjectAtCoordinates(communicationHandler, coordinates).Execute();
            communicationHandler.SleepFor(communicationHandler.GetDelayAfterCommand());
            return objectFound;
        }

        public void AddNotificationListener<T>(NotificationType notificationType, Action<T> callback, bool overwrite)
        {
            new AddNotificationListener<T>(communicationHandler, notificationType, callback, overwrite).Execute();
        }

        public void RemoveNotificationListener(NotificationType notificationType)
        {
            new RemoveNotificationListener(communicationHandler, notificationType).Execute();
        }
    }
}
