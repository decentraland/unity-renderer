using System.Collections.Generic;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    public class AltGetAllScenesCommand : AltCommand<AltGetAllScenesParams, List<string>>
    {
        public AltGetAllScenesCommand(AltGetAllScenesParams cmdParam) : base(cmdParam) { }
        public override List<string> Execute()
        {
            var sceneNames = new List<string>();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                var s = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
                sceneNames.Add(s);
            }
            return sceneNames;
        }
    }
}