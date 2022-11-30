using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltGetTextCommand : AltReflectionMethodsCommand<AltGetTextParams, string>
    {
        static readonly AltObjectProperty[] textProperties =
        {
            new AltObjectProperty("UnityEngine.UI.Text", "text"),
            new AltObjectProperty("UnityEngine.UI.InputField", "text"),
            new AltObjectProperty("TMPro.TMP_Text", "text", "Unity.TextMeshPro"),
            new AltObjectProperty("TMPro.TMP_InputField", "text", "Unity.TextMeshPro")
        };

        public AltGetTextCommand(AltGetTextParams cmdParams) : base(cmdParams)
        {
        }

        public override string Execute()
        {
            Exception exception = null;

            foreach (var property in textProperties)
            {
                try
                {
                    System.Type type = GetType(property.Component, property.Assembly);
                    return GetValueForMember(CommandParams.altObject, property.Property.Split('.'), type) as string;
                }
                catch (PropertyNotFoundException ex)
                {
                    exception = ex;
                }
                catch (ComponentNotFoundException ex)
                {
                    exception = ex;
                }
                catch (AssemblyNotFoundException ex)
                {
                    exception = ex;
                }
            }

            if (exception != null) throw exception;
            throw new Exception("Something went wrong"); // should not reach this point
        }
    }
}
