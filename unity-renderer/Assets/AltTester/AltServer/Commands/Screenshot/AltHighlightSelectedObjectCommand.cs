using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    class AltHighlightSelectedObjectCommand : AltBaseScreenshotCommand<AltHighlightObjectScreenshotParams, string>
    {

        public AltHighlightSelectedObjectCommand(ICommandHandler handler, AltHighlightObjectScreenshotParams cmdParams) : base(handler, cmdParams)
        {

        }

        public override string Execute()
        {
            var gameObject = AltRunner.GetGameObject(CommandParams.altObjectId);

            if (gameObject != null)
            {
                var color = new UnityEngine.Color(CommandParams.color.r, CommandParams.color.g, CommandParams.color.b, CommandParams.color.a);

                AltRunner._altRunner.StartCoroutine(SendScreenshotObjectHighlightedCoroutine(CommandParams.size.ToUnity(), CommandParams.quality, gameObject, color, CommandParams.width));
            }
            else
            {
                AltRunner._altRunner.StartCoroutine(SendTexturedScreenshotCoroutine(CommandParams.size.ToUnity(), CommandParams.quality));
            }
            return "Ok";
        }
    }
}
