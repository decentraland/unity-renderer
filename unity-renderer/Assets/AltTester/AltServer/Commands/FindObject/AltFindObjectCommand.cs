using System.Linq;
using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltFindObjectCommand : AltBaseClassFindObjectsCommand<AltObject>
    {
        public AltFindObjectCommand(BaseFindObjectsParams cmdParam) : base(cmdParam) { }

        public override AltObject Execute()
        {
            var path = new PathSelector(CommandParams.path);
            var foundGameObject = FindObjects(null, path.FirstBound, true, CommandParams.enabled);
            UnityEngine.Camera camera = null;
            if (!CommandParams.cameraPath.Equals("//"))
            {
                camera = GetCamera(CommandParams.cameraBy, CommandParams.cameraPath);
                if (camera == null) throw new CameraNotFoundException();
            }
            if (foundGameObject.Count() == 1)
            {
                return
                    AltRunner._altRunner.GameObjectToAltObject(foundGameObject[0], camera);
            }
            throw new NotFoundException(string.Format("Object {0} not found", CommandParams.path));
        }
    }
}
