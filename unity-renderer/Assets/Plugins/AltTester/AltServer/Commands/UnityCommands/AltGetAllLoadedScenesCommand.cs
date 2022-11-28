using System.Collections.Generic;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltGetAllLoadedScenesCommand : AltCommand<AltGetAllLoadedScenesParams, List<string>>
    {
        private readonly List<string> sceneNames = new List<string>();

        public AltGetAllLoadedScenesCommand(AltGetAllLoadedScenesParams cmdParams) : base(cmdParams) { }

        public override List<string> Execute()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name;
                sceneNames.Add(sceneName);
            }
            return sceneNames;
        }

    }
}
