using System.Collections.Generic;
using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltGetAllLoadedScenesAndObjectsCommand : AltBaseClassFindObjectsCommand<List<AltObjectLight>>
    {
        public AltGetAllLoadedScenesAndObjectsCommand(BaseFindObjectsParams cmdParams) : base(cmdParams) { }

        public override List<AltObjectLight> Execute()
        {
            List<AltObjectLight> foundObjects = new List<AltObjectLight>();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                foundObjects.Add(new AltObjectLight(scene.name));
                foreach (UnityEngine.GameObject rootGameObject in UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    if (CommandParams.enabled == false || rootGameObject.activeSelf)
                    {
                        foundObjects.Add(AltRunner._altRunner.GameObjectToAltObjectLight(rootGameObject));
                        foundObjects.AddRange(getAllChildren(rootGameObject));
                    }
                }
            }

            var doNotDestroyOnLoadObjects = AltRunner.GetDontDestroyOnLoadObjects();
            if (doNotDestroyOnLoadObjects.Length != 0)
            {
                foundObjects.Add(new AltObjectLight("DontDestroyOnLoad"));
            }
            foreach (var destroyOnLoadObject in AltRunner.GetDontDestroyOnLoadObjects())
            {
                if (CommandParams.enabled == false || destroyOnLoadObject.activeSelf)
                {
                    foundObjects.Add(AltRunner._altRunner.GameObjectToAltObjectLight(destroyOnLoadObject));
                    foundObjects.AddRange(getAllChildren(destroyOnLoadObject));
                }
            }
            return foundObjects;

        }
        private List<AltObjectLight> getAllChildren(UnityEngine.GameObject gameObject)
        {
            List<AltObjectLight> children = new List<AltObjectLight>();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                if (CommandParams.enabled == false || child.activeSelf)
                {
                    children.Add(AltRunner._altRunner.GameObjectToAltObjectLight(child));
                    children.AddRange(getAllChildren(child));
                }

            }
            return children;
        }
    }
}