namespace Altom.AltDriver.Commands
{
    public class AltGetScreenshotResponse
    {
        public AltVector2 scaleDifference;
        public AltVector3 textureSize;
        public byte[] compressedImage;

    }
    public class AltGetScreenshot : AltBaseCommand
    {
        AltGetScreenshotParams cmdParams;

        public AltGetScreenshot(IDriverCommunication commHandler, AltVector2 size, int screenShotQuality) : base(commHandler)
        {
            cmdParams = new AltGetScreenshotParams(size, screenShotQuality);
        }
        public AltTextureInformation Execute()
        {
            CommHandler.Send(cmdParams);

            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);

            var imageData = CommHandler.Recvall<AltGetScreenshotResponse>(cmdParams);
            byte[] decompressedImage = DecompressScreenshot(imageData.compressedImage);
            return new AltTextureInformation(decompressedImage, imageData.scaleDifference, imageData.textureSize);
        }
    }


    public class AltGetHighlightObjectScreenshot : AltBaseCommand
    {

        AltHighlightObjectScreenshotParams cmdParams;

        public AltGetHighlightObjectScreenshot(IDriverCommunication commHandler, int id, AltColor color, float width, AltVector2 size, int screenShotQuality) : base(commHandler)
        {
            cmdParams = new AltHighlightObjectScreenshotParams(id, color, width, size, screenShotQuality);
        }

        public AltTextureInformation Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);

            var imageData = CommHandler.Recvall<AltGetScreenshotResponse>(cmdParams);
            byte[] decompressedImage = DecompressScreenshot(imageData.compressedImage);
            return new AltTextureInformation(decompressedImage, imageData.scaleDifference, imageData.textureSize);
        }
    }


    public class AltGetHighlightObjectFromCoordinatesScreenshot : AltCommandReturningAltElement
    {
        AltHighlightObjectFromCoordinatesScreenshotParams cmdParams;


        public AltGetHighlightObjectFromCoordinatesScreenshot(IDriverCommunication commHandler, AltVector2 coordinates, AltColor color, float width, AltVector2 size, int screenShotQuality) : base(commHandler)
        {
            cmdParams = new AltHighlightObjectFromCoordinatesScreenshotParams(coordinates, color, width, size, screenShotQuality);
        }
        public AltTextureInformation Execute(out AltObject selectedObject)
        {
            CommHandler.Send(cmdParams);
            selectedObject = ReceiveAltObject(cmdParams);
            if (selectedObject != null && selectedObject.name.Equals("Null") && selectedObject.id == 0)
            {
                selectedObject = null;
            }

            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);

            var imageData = CommHandler.Recvall<AltGetScreenshotResponse>(cmdParams);
            byte[] decompressedImage = DecompressScreenshot(imageData.compressedImage);
            return new AltTextureInformation(decompressedImage, imageData.scaleDifference, imageData.textureSize);

        }
    }

}