using System;
using System.Collections;
using System.Linq;
using DCL.Controllers;
using UnityEngine;
using UnityGLTF;

namespace DCL
{
    public class LoadingScreenController
    {
        public const int MAX_DISTANCE_TO_PLAYER = 10;

        private const float LOADING_BUDGET_PER_STEP = 1 / 3f;

        private const float WAIT_BEFORE_FORCED_START = 10f;
        private const float GATHERING_TIME = 8f;
        private const int SCENES_TO_LOAD = 7; //Assumption that scenes are loaded by distance order
        private const float GLTF_TO_LOAD_PERCENTAGE = 0.65f;
        private const float POLLING_INTERVAL_TIME = 0.25f;
        private const float TIMEOUT = 15;

        public event Action OnLoadingDone = () => { };

        public bool started
        {
            get;
            private set;
        }

        private readonly SceneController sceneController;

        private GameObject loadingScreen;
        private Coroutine timeOutCoroutine;
        private LoadingScreenView loadingScreenView;

        public LoadingScreenController(SceneController sceneController, bool enableInEditor = false)
        {
            this.sceneController = sceneController;
            
            CreateLoadingScreen();

#if UNITY_EDITOR
            if(enableInEditor)
            {
                sceneController.StartCoroutine(WaitAndForceLoadingIfNoSceneMessage());
            }
            else
            {
                RemoveLoadingScreen();
                OnLoadingDone.Invoke();
            }
#else
            sceneController.StartCoroutine(WaitAndForceLoadingIfNoSceneMessage());
#endif
        }

        private void CreateLoadingScreen()
        {
            loadingScreen = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/LoadingScreen"));
            loadingScreenView = loadingScreen.GetComponent<LoadingScreenView>();
            loadingScreenView.SetNormalizedPercentage(0);
        }

        private void RemoveLoadingScreen() 
        {
            sceneController.StartCoroutine(loadingScreenView.FadeOutAndDestroy());
        }

        public void StartLoadingScreen()
        {
            started = true;
            var coroutine = sceneController.StartCoroutine(LoadingProcess());
            timeOutCoroutine = sceneController.StartCoroutine(TimeOut(TIMEOUT, coroutine));
        }

        private IEnumerator WaitAndForceLoadingIfNoSceneMessage()
        {
            float waitStart = Time.realtimeSinceStartup;

            while(!started && Time.realtimeSinceStartup < waitStart + WAIT_BEFORE_FORCED_START)
            {
                yield return new WaitForEndOfFrame();
            }

            if(!started)
                StartLoadingScreen();
        }

        private IEnumerator LoadingProcess()
        {
            yield return WaitForGathering();

            yield return WaitForScenesReady();

            yield return WaitForGLTFs();

            sceneController.StopCoroutine(timeOutCoroutine);
            RemoveLoadingScreen();

            OnLoadingDone.Invoke();
        }
        private IEnumerator WaitForGathering()
        {
            float time = 0;
            while (time < GATHERING_TIME)
            {
                time = Mathf.Min(time + Time.deltaTime, GATHERING_TIME);
                float currentPercentage = Mathf.Clamp( LOADING_BUDGET_PER_STEP * time / GATHERING_TIME, 0, LOADING_BUDGET_PER_STEP);
                loadingScreenView.SetNormalizedPercentage(currentPercentage);
                yield return null;
            }
            loadingScreenView.SetNormalizedPercentage(LOADING_BUDGET_PER_STEP);
        }

        private IEnumerator WaitForScenesReady()
        {
            Vector2 playerPos = ParcelScene.WorldToGridPosition(DCLCharacterController.i.transform.position);
            var scenesToCheck = sceneController.loadedScenes
                .Where(x => Vector2Int.Distance(x.Value.sceneData.basePosition, new Vector2Int((int)playerPos.x, (int)playerPos.y)) < 20 )
                .Select(x => x.Key).ToList();
            scenesToCheck = scenesToCheck.GetRange(0, Math.Min(scenesToCheck.Count, SCENES_TO_LOAD));

            var scenesReady = sceneController.readyScenes.Intersect(scenesToCheck).ToList();
            while (scenesReady.Count < scenesToCheck.Count)
            {
                yield return new WaitForSeconds(POLLING_INTERVAL_TIME);

                //we have to update scenesToCheck since some scenes can be removed
                scenesToCheck = sceneController.loadedScenes
                    .Where(x => Vector2Int.Distance(x.Value.sceneData.basePosition, new Vector2Int((int)playerPos.x, (int)playerPos.y)) < 20 )
                    .Select(x => x.Key).ToList();
                scenesToCheck = scenesToCheck.GetRange(0, Math.Min(scenesToCheck.Count, SCENES_TO_LOAD));
                scenesReady = sceneController.readyScenes.Intersect(scenesToCheck).ToList();

                float currentPercentage = Mathf.Clamp( LOADING_BUDGET_PER_STEP * scenesReady.Count() / scenesToCheck.Count, 0, LOADING_BUDGET_PER_STEP);
                loadingScreenView.SetNormalizedPercentage(LOADING_BUDGET_PER_STEP + currentPercentage);
            }
            loadingScreenView.SetNormalizedPercentage(2 * LOADING_BUDGET_PER_STEP);
        }

        private IEnumerator WaitForGLTFs()
        {
            int totalGLTFToProcess = GLTFComponent.totalDownloadedCount + Mathf.FloorToInt(GLTFComponent.queueCount * GLTF_TO_LOAD_PERCENTAGE);
            
            while ( GLTFComponent.totalDownloadedCount < totalGLTFToProcess)
            {
                yield return new WaitForSeconds(POLLING_INTERVAL_TIME);

                float currentPercentage = Mathf.Clamp( LOADING_BUDGET_PER_STEP * GLTFComponent.totalDownloadedCount / totalGLTFToProcess, 0, LOADING_BUDGET_PER_STEP);
                loadingScreenView.SetNormalizedPercentage(2 * LOADING_BUDGET_PER_STEP + currentPercentage);
            }
            loadingScreenView.SetNormalizedPercentage(1);
        }

        private IEnumerator TimeOut(float seconds, Coroutine loadingCoroutine)
        {
            yield return new WaitForSeconds(seconds);

            sceneController.StopCoroutine(loadingCoroutine);
            RemoveLoadingScreen();
        }
    }
}