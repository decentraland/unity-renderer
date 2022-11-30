using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltDeletePlayerPrefCommand : AltCommand<AltDeletePlayerPrefParams, string>
    {
        public AltDeletePlayerPrefCommand(AltDeletePlayerPrefParams cmdParams) : base(cmdParams)
        { }

        public override string Execute()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
            return "Ok";
        }
    }
}
