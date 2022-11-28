using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Altom.AltTester.Commands
{
    class AltSetTextCommand : AltReflectionMethodsCommand<AltSetTextParams, AltObject>
    {
        static readonly AltObjectProperty[] textProperties =
        {
            new AltObjectProperty("UnityEngine.UI.Text", "text"),
            new AltObjectProperty("UnityEngine.UI.InputField", "text"),
            new AltObjectProperty("TMPro.TMP_Text", "text", "Unity.TextMeshPro"),
            new AltObjectProperty("TMPro.TMP_InputField", "text", "Unity.TextMeshPro")
        };

        public AltSetTextCommand(AltSetTextParams cmdParams) : base(cmdParams)
        {
        }

        public override AltObject Execute()
        {
            var targetObject = AltRunner.GetGameObject(CommandParams.altObject);
            Exception exception = null;

            foreach (var property in textProperties)
            {
                try
                {
                    System.Type type = GetType(property.Component, property.Assembly);

                    string valueText = Newtonsoft.Json.JsonConvert.SerializeObject(CommandParams.value);
                    SetValueForMember(CommandParams.altObject, property.Property.Split('.'), type, valueText);
                    var uiInputFieldComp = targetObject.GetComponent<UnityEngine.UI.InputField>();
                    if (uiInputFieldComp != null)
                    {
                        uiInputFieldComp.onValueChanged.Invoke(CommandParams.value);
                        checkSubmit(uiInputFieldComp.gameObject);
#if UNITY_2021_1_OR_NEWER
                        uiInputFieldComp.onSubmit.Invoke(CommandParams.value);
#endif
                        uiInputFieldComp.onEndEdit.Invoke(CommandParams.value);
                    }
                    else
                    {
                        var tMPInputFieldComp = targetObject.GetComponent<TMPro.TMP_InputField>();
                        if (tMPInputFieldComp != null)
                        {
                            tMPInputFieldComp.onValueChanged.Invoke(CommandParams.value);
                            checkSubmit(tMPInputFieldComp.gameObject);
                            tMPInputFieldComp.onSubmit.Invoke(CommandParams.value);
                            tMPInputFieldComp.onEndEdit.Invoke(CommandParams.value);
                        }
                    }
                    return AltRunner._altRunner.GameObjectToAltObject(targetObject);
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

        private void checkSubmit(GameObject obj)
        {
            if (CommandParams.submit)
                ExecuteEvents.Execute(obj, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }
}