using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    class AltMoveMouseCommand : AltCommandWithWait<AltMoveMouseParams, string>
    {
        public AltMoveMouseCommand(ICommandHandler handler, AltMoveMouseParams cmdParams) : base(cmdParams, handler, cmdParams.wait)
        {
        }

        public override string Execute()
        {
            InputController.MoveMouse(CommandParams.coordinates.ToUnity(), CommandParams.duration, onFinish);
            return "Ok";
        }
    }
}
