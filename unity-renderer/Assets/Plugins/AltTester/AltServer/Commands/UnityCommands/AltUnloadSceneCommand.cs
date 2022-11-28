using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    class AltUnloadSceneCommand : AltCommand<AltUnloadSceneParams, string>
    {
        readonly ICommandHandler handler;

        public AltUnloadSceneCommand(ICommandHandler handler, AltUnloadSceneParams cmdParams) : base(cmdParams)
        {
            this.handler = handler;
        }

        public override string Execute()
        {
            try
            {
                var sceneLoadingOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(CommandParams.sceneName);
                if (sceneLoadingOperation == null)
                {
                    throw new CouldNotPerformOperationException("Cannot unload scene: " + CommandParams.sceneName);
                }
                sceneLoadingOperation.completed += sceneUnloaded;
            }
            catch (ArgumentException)
            {
                throw new CouldNotPerformOperationException("Cannot unload scene: " + CommandParams.sceneName);
            }

            return "Ok";
        }

        private void sceneUnloaded(UnityEngine.AsyncOperation obj)
        {
            handler.Send(ExecuteAndSerialize(() => "Scene Unloaded"));
        }
    }
}
