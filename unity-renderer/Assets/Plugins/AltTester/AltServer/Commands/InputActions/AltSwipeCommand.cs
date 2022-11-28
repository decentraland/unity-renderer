using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    class AltSwipeCommand : AltCommandWithWait<AltSwipeParams, string>
    {
        public AltSwipeCommand(ICommandHandler handler, AltSwipeParams cmdParams) : base(cmdParams, handler, cmdParams.wait)
        {
        }

        public override string Execute()
        {

            UnityEngine.Vector2[] positions = { CommandParams.start.ToUnity(), CommandParams.end.ToUnity() };
            InputController.SetMultipointSwipe(positions, CommandParams.duration, onFinish);
            return "Ok";
        }
    }
}
