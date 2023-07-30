using Cysharp.Threading.Tasks;

namespace CameraReel
{
    public class CameraReelPlugin: IPlugin
    {

        public CameraReelPlugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {

        }

        public void Dispose()
        {
            // hudController.Dispose();
        }
    }
}
