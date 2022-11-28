using System.Collections.Generic;
using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltFindObjectsLightCommand : AltBaseClassFindObjectsCommand<List<AltObjectLight>>
    {
        public AltFindObjectsLightCommand(BaseFindObjectsParams cmdParams) : base(cmdParams)
        {
        }

        public override List<AltObjectLight> Execute()
        {
            var path = new PathSelector(CommandParams.path);
            var foundObjects = new List<AltObjectLight>();
            foreach (UnityEngine.GameObject testableObject in FindObjects(null, path.FirstBound, false, CommandParams.enabled))
            {
                foundObjects.Add(AltRunner._altRunner.GameObjectToAltObjectLight(testableObject));
            }

            return foundObjects;
        }
    }
}
