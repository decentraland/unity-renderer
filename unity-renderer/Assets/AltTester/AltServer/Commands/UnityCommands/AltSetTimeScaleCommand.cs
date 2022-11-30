using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltSetTimeScaleCommand : AltCommand<AltSetTimeScaleParams, string>
    {
        public AltSetTimeScaleCommand(AltSetTimeScaleParams cmdParams) : base(cmdParams)
        {
        }

        public override string Execute()
        {
            UnityEngine.Time.timeScale = CommandParams.timeScale;
            return "Ok";
        }
    }
}
