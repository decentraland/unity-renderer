using System;

namespace Altom.AltTester
{
    public enum AltInstrumentationMode
    {
        Server,
        Proxy
    }

    /// <summary>
    /// Unity App Instrumentation settings for AltTester
    /// </summary>
    [Serializable]
    public class AltInstrumentationSettings
    {
        public AltInstrumentationMode InstrumentationMode = AltInstrumentationMode.Server;

        /// <summary>
        /// The proxy host to which the Instrumented Unity App will connect to. Used only in Proxy instrumentation mode
        /// </summary>
        public string ProxyHost = "127.0.0.1";

        /// <summary>
        /// The proxy port to which the Instrumented Unity App will connect to. Used only in Proxy instrumentation mode
        /// </summary>
        public int ProxyPort = 13000;

        public string GameName = "__default__";


        /// <summary>
        /// If true, it will show where an action happens on screen ( e.g. swipe or click )
        /// </summary>
        public bool InputVisualizer = true;

        /// <summary>
        /// If true, it will display the `AltTester` popup in Instrumented Unity App
        /// </summary>
        public bool ShowPopUp = true;

    }
}