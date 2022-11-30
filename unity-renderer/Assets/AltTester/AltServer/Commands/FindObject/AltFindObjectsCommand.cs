using System.Collections.Generic;
using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltFindObjectsCommand : AltBaseClassFindObjectsCommand<List<AltObject>>
    {
        public AltFindObjectsCommand(BaseFindObjectsParams cmdParams) : base(cmdParams) { }

        public override List<AltObject> Execute()
        {
            UnityEngine.Camera camera = null;
            if (!CommandParams.cameraPath.Equals("//"))
            {
                camera = GetCamera(CommandParams.cameraBy, CommandParams.cameraPath);
                if (camera == null) throw new CameraNotFoundException();
            }
            var path = new PathSelector(CommandParams.path);
            var foundObjects = new List<AltObject>();
            foreach (UnityEngine.GameObject testableObject in FindObjects(null, path.FirstBound, false, CommandParams.enabled))
            {
                foundObjects.Add(AltRunner._altRunner.GameObjectToAltObject(testableObject, camera));
            }

            return foundObjects;
        }
    }
}
