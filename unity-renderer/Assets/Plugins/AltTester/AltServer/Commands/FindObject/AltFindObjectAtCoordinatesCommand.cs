using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltFindObjectAtCoordinatesCommand : AltCommand<AltFindObjectAtCoordinatesParams, AltObject>
    {
        public AltFindObjectAtCoordinatesCommand(AltFindObjectAtCoordinatesParams cmdParam) : base(cmdParam) { }

        public override AltObject Execute()
        {
            UnityEngine.GameObject gameObject = FindObjectViaRayCast.FindObjectAtCoordinates(CommandParams.coordinates.ToUnity());

            if (gameObject == null) return null;

            return AltRunner._altRunner.GameObjectToAltObject(gameObject);
        }


    }
}
