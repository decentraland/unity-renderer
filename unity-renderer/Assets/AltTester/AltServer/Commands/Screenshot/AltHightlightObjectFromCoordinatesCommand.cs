using System.Collections.Generic;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;
using UnityEngine;

namespace Altom.AltTester.Commands
{
    class AltHighlightObjectFromCoordinatesCommand : AltBaseScreenshotCommand<AltHighlightObjectFromCoordinatesScreenshotParams, string>
    {
        private static List<GameObject> previousResults = null;
        private static Vector2 previousScreenCoordinates;


        public AltHighlightObjectFromCoordinatesCommand(ICommandHandler handler, AltHighlightObjectFromCoordinatesScreenshotParams cmdParams) : base(handler, cmdParams)
        {
        }

        public override string Execute()
        {
            var color = new UnityEngine.Color(CommandParams.color.r, CommandParams.color.g, CommandParams.color.b, CommandParams.color.a);

            GameObject selectedObject = getObjectAtCoordinates();

            if (selectedObject != null)
            {
                Handler.Send(ExecuteAndSerialize(() => AltRunner._altRunner.GameObjectToAltObject(selectedObject)));
                AltRunner._altRunner.StartCoroutine(SendScreenshotObjectHighlightedCoroutine(CommandParams.size.ToUnity(), CommandParams.quality, selectedObject, color, CommandParams.width));
            }
            else
            {
                Handler.Send(ExecuteAndSerialize(() => new AltObject("Null")));
                AltRunner._altRunner.StartCoroutine(SendTexturedScreenshotCoroutine(CommandParams.size.ToUnity(), CommandParams.quality));
            }
            return "Ok";
        }

        private GameObject getObjectAtCoordinates()
        {
            GameObject selectedObject = null;
            AltMockUpPointerInputModule mockUp = new AltMockUpPointerInputModule();
            var screenCoordinates = CommandParams.coordinates.ToUnity();
            var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = screenCoordinates
            };
            List<GameObject> currentResults = new List<GameObject>();
            List<UnityEngine.EventSystems.RaycastResult> hitUI;
            mockUp.GetAllRaycastResults(pointerEventData, out hitUI);
            for (int i = 0; i < hitUI.Count; i++)
            {
                currentResults.Add(hitUI[i].gameObject);
                if (previousResults == null || previousScreenCoordinates != screenCoordinates || previousResults.Count <= i || previousResults[i] != hitUI[i].gameObject)
                {
                    selectedObject = hitUI[i].gameObject;
                    break;
                }
            }

            if (selectedObject == null)
            {
                foreach (var camera in Camera.allCameras)
                {

                    Ray ray = camera.ScreenPointToRay(screenCoordinates);
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(ray);
                    if (hits.Length > 0)
                    {
                        currentResults.Add(hits[hits.Length - 1].transform.gameObject);
                        if (previousResults == null || previousScreenCoordinates != screenCoordinates || previousResults.Count < currentResults.Count || previousResults[currentResults.Count - 1] != currentResults[currentResults.Count - 1])
                        {
                            selectedObject = hits[hits.Length - 1].transform.gameObject;
                            break;
                        }
                    }
                }
            }

            previousScreenCoordinates = screenCoordinates;
            previousResults = currentResults;
            if (selectedObject == null && currentResults.Count != 0)
            {
                selectedObject = currentResults[0];
                previousResults.Clear();
                previousResults.Add(selectedObject);
            }

            return selectedObject;
        }
    }
}
