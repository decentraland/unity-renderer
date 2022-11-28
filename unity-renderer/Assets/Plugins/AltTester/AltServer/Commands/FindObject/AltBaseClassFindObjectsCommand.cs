using System.Collections.Generic;
using System.Linq;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using UnityEngine;

namespace Altom.AltTester.Commands
{
    class AltBaseClassFindObjectsCommand<T> : AltCommand<BaseFindObjectsParams, T>
    {
        protected readonly BaseFindObjectsParams FindObjectsParams;

        protected AltBaseClassFindObjectsCommand(BaseFindObjectsParams cmdParams) : base(cmdParams)
        {
            this.FindObjectsParams = cmdParams;
        }
        public override T Execute()
        {
            throw new System.NotImplementedException();
        }
        public List<UnityEngine.GameObject> FindObjects(UnityEngine.GameObject gameObject, BoundCondition boundCondition, bool singleObject, bool enabled)
        {
            if (boundCondition == null)
            {
                if (gameObject == null)
                    return new List<GameObject>();
                else
                    return new List<GameObject>() { gameObject };
            }

            if (boundCondition.Type == BoundType.Parent)
            {
                //   /name/../../name
                if (gameObject == null)
                {
                    throw new InvalidPathException("Cannot select a parent of root object");
                }
                var parent = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject : null;
                return FindObjects(parent, boundCondition.NextBound, singleObject, enabled);
            }

            List<UnityEngine.GameObject> objectsToCheck = getGameObjectsToCheck(gameObject);
            List<UnityEngine.GameObject> objectsFound = new List<UnityEngine.GameObject>();

            foreach (var objectToCheck in objectsToCheck)
            {
                GameObject nextObjectToCheck;
                if (checkValidVisibility(objectToCheck, enabled))
                {
                    nextObjectToCheck = objectMatchesConditions(objectToCheck, boundCondition, enabled);
                    if (nextObjectToCheck != null)
                    {
                        objectsFound.AddRange(FindObjects(nextObjectToCheck, boundCondition.NextBound, singleObject, enabled));
                        if (singleObject && objectsFound.Count > 0)
                        {
                            return objectsFound;
                        }
                    }
                }
                if (boundCondition.Type == BoundType.DirectChildren)
                    continue;
                objectsFound.AddRange(FindObjects(objectToCheck, boundCondition, singleObject, enabled));
                if (singleObject && objectsFound.Count != 0)//Don't search further if you already found an object
                {
                    return objectsFound;
                }
            }
            return objectsFound;
        }
        protected UnityEngine.Camera GetCamera(By cameraBy, string cameraValue)
        {

            if (cameraBy == By.NAME)
            {
                var cameraValueSplited = cameraValue.Split('/');
                var cameraName = cameraValueSplited[cameraValueSplited.Length - 1];
                return UnityEngine.Camera.allCameras.ToList().Find(c => c.name.Equals(cameraName));

            }
            else
            {
                var cameraValueProcessed = new PathSelector(cameraValue);
                var gameObjectsCameraFound = FindObjects(null, cameraValueProcessed.FirstBound, false, true);
                return UnityEngine.Camera.allCameras.ToList().Find(c => gameObjectsCameraFound.Find(d => c.gameObject.GetInstanceID() == d.GetInstanceID()));
            }

        }
        private bool checkValidVisibility(GameObject objectToCheck, bool enabled)
        {
            return !enabled || (enabled && objectToCheck.activeInHierarchy);
        }
        private GameObject objectMatchesConditions(GameObject objectToCheck, BoundCondition boundCondition, bool enabled)
        {
            var currentCondition = boundCondition.FirstSelector;
            GameObject objectChild = null;
            GameObject objectMatched = null;
            while (currentCondition != null)
            {

                objectMatched = currentCondition.MatchCondition(objectToCheck, enabled);
                if (objectMatched == null)
                {
                    return null;
                }
                if (objectMatched != objectToCheck)
                {
                    objectChild = objectMatched;
                }
                currentCondition = currentCondition.NextSelector;
            }
            return objectChild != null ? objectChild : objectMatched;
        }


        private List<UnityEngine.GameObject> getGameObjectsToCheck(UnityEngine.GameObject gameObject)
        {
            List<UnityEngine.GameObject> objectsToCheck;
            if (gameObject == null)
            {
                objectsToCheck = getAllRootObjects();
            }
            else
            {
                objectsToCheck = getAllChildren(gameObject);
            }
            return objectsToCheck;
        }

        private List<UnityEngine.GameObject> getAllChildren(UnityEngine.GameObject gameObject)
        {
            List<UnityEngine.GameObject> objectsToCheck = new List<UnityEngine.GameObject>();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                objectsToCheck.Add(gameObject.transform.GetChild(i).gameObject);
            }
            return objectsToCheck;
        }

        private List<UnityEngine.GameObject> getAllRootObjects()
        {
            List<UnityEngine.GameObject> objectsToCheck = new List<UnityEngine.GameObject>();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                foreach (UnityEngine.GameObject rootGameObject in UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    objectsToCheck.Add(rootGameObject);
                }
            }
            foreach (var destroyOnLoadObject in AltRunner.GetDontDestroyOnLoadObjects())
            {
                objectsToCheck.Add(destroyOnLoadObject);

            }
            return objectsToCheck;
        }


    }
}
