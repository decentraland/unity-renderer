using System;

namespace Altom.AltDriver.Commands
{
    [Obsolete]
    public struct AltObjectAction
    {
        public string Component;
        public string Method;
        public string Parameters;
        public string TypeOfParameters;

        public AltObjectAction(string component = "", string method = "", string parameters = "", string typeOfParameters = "", string assembly = "")
        {
            Component = component;
            Method = method;
            Parameters = parameters;
            TypeOfParameters = typeOfParameters;
            Assembly = assembly;
        }


        public string Assembly;
    }
}