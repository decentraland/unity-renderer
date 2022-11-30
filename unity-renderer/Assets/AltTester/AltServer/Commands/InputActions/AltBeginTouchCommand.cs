using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltBeginTouchCommand : AltCommand<AltBeginTouchParams, int>
    {
        public AltBeginTouchCommand(AltBeginTouchParams cmdParams) : base(cmdParams)
        {

        }
        public override int Execute()
        {
            return InputController.BeginTouch(CommandParams.coordinates.ToUnity());
        }
    }
}
