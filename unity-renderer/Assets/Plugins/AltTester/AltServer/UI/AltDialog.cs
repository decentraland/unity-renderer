using System;
using Altom.AltTester;
using Altom.AltTester.Communication;
using Altom.AltTester.Logging;

namespace Altom.AltTester.UI
{
    public class AltDialog : UnityEngine.MonoBehaviour
    {
        private static readonly NLog.Logger logger = ServerLogManager.Instance.GetCurrentClassLogger();

        private readonly UnityEngine.Color SUCCESS_COLOR = new UnityEngine.Color32(0, 165, 36, 255);
        private readonly UnityEngine.Color WARNING_COLOR = new UnityEngine.Color32(255, 255, 95, 255);
        private readonly UnityEngine.Color ERROR_COLOR = new UnityEngine.Color32(191, 71, 85, 255);

        [UnityEngine.SerializeField]
        public UnityEngine.GameObject Dialog = null;

        [UnityEngine.SerializeField]
        public UnityEngine.UI.Text TitleText = null;

        [UnityEngine.SerializeField]
        public UnityEngine.UI.Text MessageText = null;

        [UnityEngine.SerializeField]
        public UnityEngine.UI.Button CloseButton = null;

        [UnityEngine.SerializeField]
        public UnityEngine.UI.Image Icon = null;

        [UnityEngine.SerializeField]
        public UnityEngine.UI.InputField HostInputField = null;

        [UnityEngine.SerializeField]
        public UnityEngine.UI.InputField PortInputField = null;

        [UnityEngine.SerializeField]
        public UnityEngine.UI.InputField GameNameInputField = null;

        [UnityEngine.SerializeField]
        public UnityEngine.UI.Button RestartButton = null;

        private ICommunication communication;

        public AltInstrumentationSettings InstrumentationSettings { get { return AltRunner._altRunner.InstrumentationSettings; } }

        private readonly AltResponseQueue _updateQueue = new AltResponseQueue();
        private bool wasConnectedBeforeToProxy = false;

        protected void Start()
        {
            SetUpHostInputField();
            SetUpPortInputField();
            SetUpGameNameInputField();

            SetUpRestartButton();

            Dialog.SetActive(InstrumentationSettings.ShowPopUp);
            CloseButton.onClick.AddListener(ToggleDialog);
            Icon.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleDialog);
            TitleText.text = "AltTester v." + AltRunner.VERSION;

            StartAltTester();
        }

        protected void Update()
        {
            _updateQueue.Cycle();
        }

        protected void OnApplicationQuit()
        {
            cleanUp();
        }

        public void OnPortInputFieldValueChange(string value)
        {
            // Allow only positive numbers.
            if (value == "-")
            {
                PortInputField.text = "";
            }
        }

        public void SetUpHostInputField()
        {
            HostInputField.text = InstrumentationSettings.ProxyHost;
        }

        public void SetUpPortInputField()
        {
            PortInputField.text = InstrumentationSettings.ProxyPort.ToString();
            PortInputField.onValueChanged.AddListener(OnPortInputFieldValueChange);
            PortInputField.characterValidation = UnityEngine.UI.InputField.CharacterValidation.Integer;
        }

        public void SetUpGameNameInputField()
        {
            GameNameInputField.text = InstrumentationSettings.GameName;
        }

        private void OnRestartButtonPress()
        {
            logger.Debug("Restart the AltTester.");

            if (Uri.CheckHostName(HostInputField.text) != UriHostNameType.Unknown)
            {
                InstrumentationSettings.ProxyHost = HostInputField.text;
            }
            else
            {
                setDialog("The host should be a valid host.", ERROR_COLOR, true);
                return;
            }

            int port;
            if (Int32.TryParse(PortInputField.text, out port) && port > 0 && port <= 65535)
            {
                InstrumentationSettings.ProxyPort = port;
            }
            else
            {
                setDialog("The port number should be beteween 1 and 65535.", ERROR_COLOR, true);
                return;
            }

            try
            {
                RestartAltTester();
            }
            catch (Exception ex)
            {
                setDialog("An unexpected error occurred while restarting the AltTester.", ERROR_COLOR, true);
                logger.Error("An unexpected error occurred while restarting the AltTester.");
                logger.Error(ex.GetType().ToString(), ex.Message);
            }
        }

        public void SetUpRestartButton()
        {
            RestartButton.onClick.AddListener(OnRestartButtonPress);
        }

        public void ToggleDialog()
        {
            Dialog.SetActive(!Dialog.activeSelf);
        }

        private void setDialog(string message, UnityEngine.Color color, bool visible)
        {
            Dialog.SetActive(visible);
            MessageText.text = message;
            Dialog.GetComponent<UnityEngine.UI.Image>().color = color;
        }

        private void StartAltTester()
        {
            startProxyCommProtocol();
        }

        private void StopAltTester()
        {
            logger.Debug("Stopping AltTester.");
            if (communication != null)
            {
                communication.Stop();
            }
        }

        private void RestartAltTester()
        {
            StopAltTester();
            StartAltTester();
        }

        #region proxy mode comm protocol

        private void initProxyCommProtocol()
        {
            var cmdHandler = new CommandHandler();

#if UNITY_WEBGL && !UNITY_EDITOR
            communication = new WebSocketWebGLCommunication(cmdHandler, InstrumentationSettings.ProxyHost, InstrumentationSettings.ProxyPort);
#else
            communication = new WebSocketClientCommunication(cmdHandler, InstrumentationSettings.ProxyHost, InstrumentationSettings.ProxyPort, InstrumentationSettings.GameName);
#endif
            communication.OnConnect += onProxyConnect;
            communication.OnDisconnect += onProxyDisconnect;
            communication.OnError += onError;
        }

        private void startProxyCommProtocol()
        {
            initProxyCommProtocol();

            try
            {
                if (communication == null || !communication.IsListening) // start only if it is not already listening
                {
                    communication.Start();
                }
                if (!communication.IsConnected) // display dialog only if not connected
                    onStart();
            }
            catch (UnhandledStartCommError ex)
            {
                setDialog("An unexpected error occurred while starting the communication protocol.", ERROR_COLOR, true);
                logger.Error(ex.InnerException, "An unexpected error occurred while starting the communication protocol.");
            }
            catch (Exception ex)
            {
                setDialog("An unexpected error occurred while starting the communication protocol.", ERROR_COLOR, true);
                logger.Error(ex, "An unexpected error occurred while starting the communication protocol.");
            }
        }

        private void onStart()
        {
            setDialog("Connected to AltProxy on " + InstrumentationSettings.ProxyHost + ":" + InstrumentationSettings.ProxyPort + " with game name " + InstrumentationSettings.GameName, SUCCESS_COLOR, Dialog.activeSelf || wasConnectedBeforeToProxy);
            wasConnectedBeforeToProxy = false;
        }

        private void onProxyConnect()
        {
            string message = "Connected to AltProxy on " + InstrumentationSettings.ProxyHost + ":" + InstrumentationSettings.ProxyPort + " with game name " + InstrumentationSettings.GameName;
#if ALTTESTER && ENABLE_LEGACY_INPUT_MANAGER
            Input.UseCustomInput = true;
            UnityEngine.Debug.Log("Custom input: " + Input.UseCustomInput);
#endif
            _updateQueue.ScheduleResponse(() =>
            {
#if ALTTESTER && ENABLE_INPUT_SYSTEM
                NewInputSystem.DisableDefaultDevicesAndEnableAltDevices();
#endif
                setDialog(message, SUCCESS_COLOR, false);
                wasConnectedBeforeToProxy = true;
            });
        }

        private void onProxyDisconnect()
        {
#if ALTTESTER && ENABLE_LEGACY_INPUT_MANAGER
            Input.UseCustomInput = false;
            UnityEngine.Debug.Log("Custom input: " + Input.UseCustomInput);
#endif
            _updateQueue.ScheduleResponse(() =>
            {
#if ALTTESTER && ENABLE_INPUT_SYSTEM
                NewInputSystem.EnableDefaultDevicesAndDisableAltDevices();

#endif
                startProxyCommProtocol();

            });
        }

        #endregion

        private void onError(string message, Exception ex)
        {
            logger.Error(message);
            if (ex != null)
            {
                logger.Error(ex);
            }
        }

        private void cleanUp()
        {
            logger.Debug("Stopping communication protocol");
            if (communication != null)
            {
                communication.Stop();
            }
        }
    }
}