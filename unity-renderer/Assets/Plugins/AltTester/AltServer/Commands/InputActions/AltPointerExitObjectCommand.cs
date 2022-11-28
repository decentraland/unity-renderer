using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltPointerExitObjectCommand : AltCommand<AltPointerExitObjectParams, AltObject>
    {

        public AltPointerExitObjectCommand(AltPointerExitObjectParams cmdParams) : base(cmdParams)
        {
        }

        public override AltObject Execute()
        {
            var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            UnityEngine.GameObject gameObject = AltRunner.GetGameObject(CommandParams.altObject);
            UnityEngine.EventSystems.ExecuteEvents.Execute(gameObject, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerExitHandler);
            var camera = AltRunner._altRunner.FoundCameraById(CommandParams.altObject.idCamera);

            return camera != null ?
                AltRunner._altRunner.GameObjectToAltObject(gameObject, camera) :
                AltRunner._altRunner.GameObjectToAltObject(gameObject);
        }
    }
}
