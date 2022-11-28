using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltGetStringKeyPlayerPrefCommand : AltCommand<AltGetKeyPlayerPrefParams, string>
    {
        public AltGetStringKeyPlayerPrefCommand(AltGetKeyPlayerPrefParams cmdParams) : base(cmdParams)
        {
        }

        public override string Execute()
        {
            if (UnityEngine.PlayerPrefs.HasKey(CommandParams.keyName))
            {
                return UnityEngine.PlayerPrefs.GetString(CommandParams.keyName);
            }
            throw new NotFoundException(string.Format("PlayerPrefs key {0} not found", CommandParams.keyName));
        }
    }

    class AltGetFloatKeyPlayerPrefCommand : AltCommand<AltGetKeyPlayerPrefParams, float>
    {
        public AltGetFloatKeyPlayerPrefCommand(AltGetKeyPlayerPrefParams cmdParams) : base(cmdParams)
        {
        }

        public override float Execute()
        {
            if (UnityEngine.PlayerPrefs.HasKey(CommandParams.keyName))
            {
                return UnityEngine.PlayerPrefs.GetFloat(CommandParams.keyName);
            }
            throw new NotFoundException(string.Format("PlayerPrefs key {0} not found", CommandParams.keyName));
        }
    }

    class AltGetIntKeyPlayerPrefCommand : AltCommand<AltGetKeyPlayerPrefParams, int>
    {
        public AltGetIntKeyPlayerPrefCommand(AltGetKeyPlayerPrefParams cmdParams) : base(cmdParams)
        {
        }

        public override int Execute()
        {
            if (UnityEngine.PlayerPrefs.HasKey(CommandParams.keyName))
            {
                return UnityEngine.PlayerPrefs.GetInt(CommandParams.keyName);
            }
            throw new NotFoundException(string.Format("PlayerPrefs key {0} not found", CommandParams.keyName));
        }
    }
}
