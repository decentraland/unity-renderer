using System.Collections.Generic;
using System.Linq;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using UnityEngine;

namespace Altom.AltTester.Commands
{
    class AltGetAllCamerasCommand : AltCommand<CommandParams, List<AltObject>>
    {
        private readonly bool onlyActiveCameras;

        public AltGetAllCamerasCommand(AltGetAllCamerasParams cmdParams) : base(cmdParams)
        {
            this.onlyActiveCameras = false;
        }
        public AltGetAllCamerasCommand(AltGetAllActiveCamerasParams cmdParams) : base(cmdParams)
        {
            this.onlyActiveCameras = true;
        }
        public override List<AltObject> Execute()
        {
            var cameras = Object.FindObjectsOfType<Camera>();
            var cameraObjects = new List<AltObject>();
            if (onlyActiveCameras)
            {
                cameraObjects.AddRange(from Camera camera in cameras
                                       where camera.enabled == true
                                       select AltRunner._altRunner.GameObjectToAltObject(camera.gameObject));
            }
            else
            {
                cameraObjects.AddRange(from Camera camera in cameras
                                       select AltRunner._altRunner.GameObjectToAltObject(camera.gameObject));
            }
            return cameraObjects;
        }
    }
}
